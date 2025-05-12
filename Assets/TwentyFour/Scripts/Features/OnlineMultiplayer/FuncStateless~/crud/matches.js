function getDifferenceInSeconds(utcTime1, utcTime2) {
  // 将 UTC 时间字符串转换为 Date 对象
  const date1 = new Date(utcTime1);
  const date2 = new Date(utcTime2);

  // 计算时间差（毫秒）
  const differenceInMillis = Math.abs(date2 - date1);

  // 将毫秒转换为秒
  const differenceInSeconds = Math.floor(differenceInMillis / 1000);

  return differenceInSeconds;
}

const crud = {
  async set_match(client, event, context) {
    const { body } = event;
    const matchData = JSON.parse(body);
    let result = null;
    try {
      // 开启事务
      await client.query("BEGIN");

      // 1. 插入比赛主数据
      const matchInsert = `
        INSERT INTO matches 
        (room_id, start_time, end_time, stages, all_resolved,
        red_team_progress, blue_team_progress, is_robot_room, battle_mode, tournament_slug_name)
        VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10)
        RETURNING match_id`;

      const matchValues = [
        matchData.roomID,
        matchData.startTime,
        matchData.endTime,
        JSON.stringify(matchData.stages),
        matchData.allResolved,
        JSON.stringify(matchData.redTeamProgress),
        JSON.stringify(matchData.blueTeamProgress),
        matchData.isRobotRoom || false,
        matchData.battleMode || 0, // 新增字段
        matchData.tournamentSlugName || "",
      ];

      const {
        rows: [{ match_id }],
      } = await client.query(matchInsert, matchValues);

      // 2. 插入玩家数据
      const players = [
        ...matchData.redTeamProgress.teamPlayers.map((p) => ({
          ...p,
          team: "RED",
        })),
        ...matchData.blueTeamProgress.teamPlayers.map((p) => ({
          ...p,
          team: "BLUE",
        })),
      ];

      const playerInsert = `
          INSERT INTO match_players 
          (match_id, persona_id, team, current_index, 
           previous_score, current_score, current_tier, previous_tier, is_robot, battle_result)
          VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10)`;

      let matchDuration = getDifferenceInSeconds(
        matchData.startTime,
        matchData.endTime
      );
      matchDuration -= 5;
      if(matchDuration < 0) matchDuration = 0;
      if(matchDuration > 180) matchDuration = 180

      await Promise.all(
        players.map(
          (p) =>
            new Promise(async (resolve, reject) => {
              console.log(`current player ${p.uniqueId}, is robot: ${p.isRobot}`);

              if(p.isRobot) {
                resolve(1);
                return;
              }
              await client.query(playerInsert, [
                match_id,
                p.uniqueId,
                p.team,
                p.currentIndex,
                p.previousScore,
                p.currentScore,
                p.currentTier,
                p.previousTier,
                p.isRobot || false,
                p.battleResult,
              ]);


              // 3.个人数据统计
              const playerProperties = p.properties;

              // 计算总解题数
              const previousResolvedCount =
                parseInt(playerProperties.totalSolved) || 0;
              const incrementResolvedCount = p.resolvedCount || 0;
              const totalResolvedCount =
                incrementResolvedCount + previousResolvedCount;

              // 计算总时长
              const previousTime = parseInt(playerProperties.totalTime) || 0;
              const incrementTime = matchDuration;
              const totalTime = previousTime + incrementTime;

              // 计算最高分数和段位
              const previousHighest = parseInt(playerProperties.battle_highest_score) || 0
              let highestScore = 0
              let highestTier = ""
              if(previousHighest > p.currentScore) {
                highestScore = previousHighest
                highestTier = playerProperties.battle_highest_tier
              } else {
                highestScore = p.currentScore
                highestTier = p.currentTier
              }

              await client.query(
                `
                INSERT INTO player_info (persona_id, total_resolved, total_time, current_score, current_tier, highest_score, highest_tier)
                VALUES ($1, $3, $5, $6, $7, $8, $9)
                ON CONFLICT (persona_id)
                DO UPDATE SET 
                total_resolved = player_info.total_resolved + $2,
                total_time = player_info.total_time + $4,
                current_score = $6,
                current_tier = $7,
                highest_score = GREATEST(player_info.highest_score, $8),
                highest_tier = CASE
                    WHEN $8 > player_info.highest_score THEN $9
                    ELSE player_info.highest_tier
                  END
                `,
                [
                  p.uniqueId,
                  incrementResolvedCount,
                  totalResolvedCount,
                  incrementTime,
                  totalTime,
                  p.currentScore,
                  p.currentTier,
                  highestScore,
                  highestTier
                ]
              );
              resolve(1);
            })
        )
      );

      // 4. 插入解决记录
      const resolveInsert = `
          INSERT INTO resolved_records 
          (match_id, question_index, resolved_team, 
           resolved, resolved_persona_id, resolved_time, cost_time)
          VALUES ($1, $2, $3, $4, $5, $6, $7)`;

      await Promise.all(
        matchData.resolvedStatus.map((rs, index) =>
          client.query(resolveInsert, [
            match_id,
            index,
            rs.resolvedTeam,
            rs.resolved,
            rs.resolvedPersonaID,
            rs.resolvedTime,
            rs.costTime,
          ])
        )
      );

      result = { success: true, match_id };
      await client.query("COMMIT");
    } catch (err) {
      await client.query("ROLLBACK");
      throw err;
    } finally {
      await client.release();
    }
    return result;
  },
  async get_matches(client, event, context) {
    const { queryString } = event;
    let { uniqueId, page = 1, pageSize = 10 } = queryString;
    if (!uniqueId) throw new Error(`unrecognized uniqueId: ${uniqueId}`);
    let queryResult = null;

    // 参数校验
    page = Math.max(1, parseInt(page));
    pageSize = Math.min(Math.max(1, parseInt(pageSize)), 100); // 限制最大100条/页
    const offset = (page - 1) * pageSize;

    try {
      const query = `
        SELECT 
            m.match_id,
            m.room_id,
            m.start_time,
            m.end_time,
            m.battle_mode,
            m.is_robot_room,
            m.red_team_progress,
            m.blue_team_progress,
            mp.team AS player_team,
            m.stages AS questions,
            jsonb_agg(
                jsonb_build_object(
                    'questionIndex', rr.question_index,
                    'resolved', rr.resolved,
                    'resolveTime', rr.resolved_time,
                    'resolvePersonaId', rr.resolved_persona_id,
                    'costTime', rr.cost_time
                ) ORDER BY rr.question_index
            ) AS resolved_history
        FROM matches m
        JOIN match_players mp ON m.match_id = mp.match_id
        LEFT JOIN resolved_records rr 
            ON m.match_id = rr.match_id 
        WHERE mp.persona_id = $1
        GROUP BY 
            m.match_id, 
            mp.team
        ORDER BY m.start_time DESC
        LIMIT $2 OFFSET $3`;

      const { rows } = await client.query(query, [uniqueId, pageSize, offset]);

      // 获取总数（可选）
      const countQuery = `
            SELECT COUNT(DISTINCT m.match_id) 
            FROM matches m
            JOIN match_players mp ON m.match_id = mp.match_id
            WHERE mp.persona_id = $1`;

      const {
        rows: [{ count }],
      } = await client.query(countQuery, [uniqueId]);

      // 处理结果集
      const total = parseInt(count);
      const totalPages = Math.ceil(total / pageSize);

      if (page > totalPages) {
        throw new Error("");
      }
      queryResult = {
        data: rows.map((row) => ({
          matchId: row.match_id,
          roomId: row.room_id,
          startTime: row.start_time,
          endTime: row.end_time,
          playerTeam: row.player_team,
          redTeamProgress: row.red_team_progress,
          blueTeamProgress: row.blue_team_progress,
          battleMode: row.battle_mode,
          questions: row.questions,
          resolvedHistory: row.resolved_history
            .filter((r) => r.resolved !== null)
            .sort((a, b) => a.question_index - b.question_index),
        })),
        pagination: {
          total,
          currentPage: page,
          pageSize,
          totalPages,
        },
      };
    } finally {
      await client.release();
    }
    return queryResult;
  },
};

module.exports = crud;
