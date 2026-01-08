export function isNumber(value: string) : boolean {
    return !isNaN(Number(value))
}

export function toNumberOrNull(value: string | null): number | null {
    if (value === null || isNaN(Number(value))) {
        return null;
    }
    return Number(value);
}

export function toNumberOrZero(value: string | null): number {
    if (value === null || isNaN(Number(value))) {
        return 0;
    }
    return Number(value);
}