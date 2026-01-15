import { JSX } from "react";

export function Counter(): JSX.Element {
  const number = Count(5, 10);

  function Count(a: number, b: number): number {
    return a + b;
    }

  return <div>Counter - {number}</div>;
}
