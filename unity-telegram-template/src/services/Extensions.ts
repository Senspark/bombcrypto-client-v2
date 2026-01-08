export function assertIsError(error: unknown): asserts error is Error {
    // if you have nodejs assert: 
    // assert(error instanceof Error);
    // otherwise
    if (!(error instanceof Error)) {
        throw error
    }
}