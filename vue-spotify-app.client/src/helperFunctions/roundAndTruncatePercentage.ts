export default function RoundAndTruncatePercentage(value: number, decimalPlaces: number = 0): number {
  // Round the value to the nearest whole number
  const roundedValue = Math.round(value * Math.pow(10, decimalPlaces)) / Math.pow(10, decimalPlaces);
  // Truncate the value to the specified number of decimal places
  return Number(roundedValue.toFixed(decimalPlaces));
}