export default function ConvertMilisecondsToMinutesAndSeconds (value: number){
  const totalSeconds = Math.round(value / 1000);
    const minutes = Math.floor(totalSeconds / 60);
    const seconds = totalSeconds % 60;

    if(seconds < 10)
      return `${minutes}:0${seconds}`;
  return `${minutes}:${seconds}`;
}
