const fs = require('fs');
const { text } = require('stream/consumers');

function calculate24(numbers) {
    const target = 24;
    const operators = ['+', '-', '*', '/'];

    function evaluate(a, b, op) {
        switch (op) {
            case '+': return a + b;
            case '-': return a - b;
            case '*': return a * b;
            case '/': return b !== 0 ? a / b : null; // 避免除以零
        }
    }

    function permute(arr) {
        if (arr.length === 1) return arr;

        const results = [];
        for (let i = 0; i < arr.length; i++) {
            const current = arr[i];
            const remaining = arr.slice(0, i).concat(arr.slice(i + 1));
            const perms = permute(remaining);
            for (const perm of perms) {
                results.push([current].concat(perm));
            }
        }
        return results;
    }

    function backtrack(nums) {
        if (nums.length === 1) {
            if (Math.abs(nums[0] - target) < 1e-6) {
                return true;
            }
            return false;
        }

        for (let i = 0; i < nums.length; i++) {
            for (let j = 0; j < nums.length; j++) {
                if (i !== j) {
                    const a = nums[i];
                    const b = nums[j];
                    const newNums = nums.filter((_, index) => index !== i && index !== j);

                    for (const op of operators) {
                        const result = evaluate(a, b, op);
                        if (result !== null) {
                            const nextNums = newNums.concat(result);
                            if (backtrack(nextNums)) {
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    const allPermutations = permute(numbers);
    for (const perm of allPermutations) {
        if (backtrack(perm)) {
            return true;
        }
    }
    return false;
}

// 示例输入
// const numbers = [4, 1, 8, 7];
// const canMake24 = calculate24(numbers);
// console.log(canMake24 ? '可以计算出 24 点' : '无法计算出 24 点');


const MinMumber = 1;
const MaxNumber = 13;
let list = [];
const advance = 10;

for(let i = MinMumber; i <= MaxNumber; i += 1) {
    for(let j = i; j <= MaxNumber; j += 1) {
        for(let k = j; k <= MaxNumber; k += 1) {
            for(let l = k; l <= MaxNumber; l += 1) {
                const arr = [i, j,k, l];
                const canMake24 = calculate24(arr);
                if(canMake24) {
                   // if(i > advance || j > advance || k > advance || l > advance)
                    list.push(arr);
                }
            }
        }
    }
}

let textContent = JSON.stringify(list);
console.log(list.length);

// 替换为 lua 格式
textContent = textContent.replaceAll("[", "{")
textContent = textContent.replaceAll("]", "}")
textContent = `local module = ${textContent}\nreturn module`

fs.writeFile("../Data/AllBattleData.lua", textContent, (err) => {
    if(err) {
        console.log("失败");
    } else {
        console.log("成功")
    }
})