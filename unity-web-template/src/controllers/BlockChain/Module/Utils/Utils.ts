async function sleep(ms: number): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms));
}

/**
 * @param host - The host to ping
 * @returns The ping time in milliseconds
 */
async function ping(host: string): Promise<number> {
    return new Promise((resolve) => {
        const img = new Image();
        const pong = () => {
            const endTime = new Date().getTime();
            const result = endTime - startTime;
            resolve(result);
        }
        const startTime = new Date().getTime();
        img.src = host;
        img.onload = pong;
        img.onerror = pong;
        setTimeout(() => {
            resolve(-1);
        }, 1500);
    });
}

function convertBNArrayToNumberArray(arr: bigint[]): string[] {
    return arr.map(e => e.toString());
}

function convertBN2DArrayToNumber2DArray(arr: bigint[][]): string[][] {
    return arr.map(e => convertBNArrayToNumberArray(e));
}

// function createTaskCompleteSource(): [Promise<void>, () => void] {
//     let resolver: () => void;
//     return [
//         new Promise<void>((_resolve) => {
//             resolver = _resolve;
//         }),
//         resolver!
//     ];
// }

function createTaskCompleteSource<T>(): [Promise<T>, (value: T) => void] {
    let resolver: (value: T) => void;
    return [
        new Promise<T>((_resolve) => {
            resolver = _resolve;
        }),
        resolver!
    ];
}

function shuffle<T>(array: T[]): T[] {
    let currentIndex = array.length, randomIndex;

    // While there remain elements to shuffle.
    while (currentIndex != 0) {

        // Pick a remaining element.
        randomIndex = Math.floor(Math.random() * currentIndex);
        currentIndex--;

        // And swap it with the current element.
        [array[currentIndex], array[randomIndex]] = [
            array[randomIndex], array[currentIndex]];
    }

    return array;
}

export { sleep, ping, convertBN2DArrayToNumber2DArray, convertBNArrayToNumberArray, createTaskCompleteSource, shuffle };