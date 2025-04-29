function getWinRate(winCount, totalCount) {
  const wins = parseFloat(winCount);
  const total = parseFloat(totalCount);

  if (total < 1) {
      return 0; // 处理总数为零的情况
  }

  const rate = (wins / total) * 100; // 计算胜率
  return rate.toFixed(2); // 保留两位小数并添加百分号
}

const tournament = {
  async getScore(client, queryString) {
    const {tournamentSlugName, uniqueId, page = 1, pageSize = 10} = queryString;
    console.log(tournamentSlugName, uniqueId)

    // 获取「uniqueId」在「tournamentSlugName」锦标赛中的所有记录
    try {
      const query = `
      WITH
      win_statics AS (SELECT
        COUNT(*) AS total_count,
        COUNT(CASE WHEN mp.battle_result > 0 THEN 1 END) AS win_count,
        COUNT(CASE WHEN mp.battle_result = 2 THEN 1 END) AS complete_win_count,
        SUM(EXTRACT(EPOCH FROM (m.end_time - m.start_time))) AS duration
      FROM matches m
      INNER JOIN match_players mp
        ON m.match_id = mp.match_id
      WHERE 
        m.tournament_slug_name = $1
        AND mp.persona_id = $2),
      avg_statics AS (
        SELECT 
          ROUND(AVG(cost_time::numeric), 2) AS avg_time,
          COUNT(*) AS resolved_count
        FROM matches m
        LEFT JOIN resolved_records rr
          ON m.match_id = rr.match_id
        WHERE 
          m.tournament_slug_name = $1
          AND rr.resolved_persona_id = $2
          AND rr.cost_time <> 0
      )

      SELECT
        total_count,
        win_count,
        complete_win_count,
        avg_time,
        duration,
        resolved_count
      FROM
        win_statics,
        avg_statics;
    `;

    const { rows } = await client.query(query, [tournamentSlugName, uniqueId]);
    console.log("查询结果")
    console.log(JSON.stringify(rows, null, 2))
    const result = rows.map( row => {
      return {
        totalCount: row.total_count,
        winCount: row.win_count,
        completeWinCount: row.complete_win_count,
        avgTime: row.resolved_count !== "0" ? (row.duration / row.resolved_count).toFixed(2) : "-",
        winRate: getWinRate(row.win_count, row.total_count),
        // avgTimeOld: row.avg_time,
        // resolved_count: row.resolved_count,
        // duration: row.duration
      }
    });
    console.log(result)
    return result;
    } catch(error) {
      console.log(error)
    } finally {
      await client.release();
    }

  },
  async getLeaderboard(client, queryString) {
    const {tournamentSlugName, uniqueId, page = 1, pageSize = 10} = queryString;
    console.log(tournamentSlugName, uniqueId)

    // 获取「uniqueId」在「tournamentSlugName」锦标赛中的所有记录
    try {
      const query = `
      WITH
      win_statics AS (SELECT
        mp.persona_id AS persona_id,
        COUNT(*) AS total_count,
        COUNT(CASE WHEN mp.battle_result > 0 THEN 1 END) AS win_count,
        COUNT(CASE WHEN mp.battle_result = 2 THEN 1 END) AS complete_win_count
      FROM matches m
      INNER JOIN match_players mp
        ON m.match_id = mp.match_id
      WHERE 
        m.tournament_slug_name = $1
      GROUP BY persona_id),
      avg_statics AS (
        SELECT
          resolved_persona_id,
          ROUND(AVG(cost_time), 2) AS avg_time,
          COUNT(*) AS resolved_count,
          MIN(cost_time) AS min_time
        FROM matches m
        LEFT JOIN resolved_records rr
          ON m.match_id = rr.match_id
        WHERE 
          m.tournament_slug_name = $1
          AND rr.cost_time <> 0
        GROUP BY resolved_persona_id),
      rank_data AS (
        SELECT
          persona_id,
          total_count,
          win_count,
          min_time,
          ROUND(win_count / NULLIF(total_count, 0), 2) AS win_rate,
          complete_win_count,
          avg_time
        FROM
          win_statics ws
        INNER JOIN avg_statics avg
          ON ws.persona_id = avg.resolved_persona_id)
      
        SELECT 
          (SELECT persona_id 
          FROM rank_data 
          ORDER BY complete_win_count DESC 
          LIMIT 1) AS max_complete_win_persona_id,
          
          (SELECT complete_win_count 
          FROM rank_data 
          ORDER BY complete_win_count DESC 
          LIMIT 1) AS max_complete_win_count,

          (SELECT persona_id 
          FROM rank_data 
          ORDER BY avg_time ASC 
          LIMIT 1) AS min_avg_time_persona_id,

          (SELECT avg_time 
          FROM rank_data 
          ORDER BY avg_time ASC 
          LIMIT 1) AS min_avg_time,

          (SELECT persona_id 
          FROM rank_data 
          ORDER BY min_time ASC 
          LIMIT 1) AS min_time_persona_id,

          (SELECT min_time 
          FROM rank_data 
          ORDER BY min_time ASC 
          LIMIT 1) AS min_time;
    `;

    const { rows } = await client.query(query, [tournamentSlugName]);
    console.log("查询结果")
    console.log(JSON.stringify(rows, null, 2))

    } catch(error) {
      console.log(error)
    } finally {
      await client.release();
    }

  }
}

module.exports = tournament

