# Prisoners Dilemma

run with

```sh
dotnet run --project SocietalConstructionTool -- ./Programs/PrisonersDilemma/*
```

## Benchmark

Running ~100 generations with 8 players
- 3m05 while printing
  - Result unknown, was lost in the output due to length
- 3m04 without printing
  - Result::Final(player: 0, points: 1956)
    Result::Final(player: 1, points: 1222)
    Result::Final(player: 2, points: 1884)
    Result::Final(player: 3, points: 1765)
    Result::Final(player: 4, points: 1797)
    Result::Final(player: 5, points: 998)
    Result::Final(player: 6, points: 1642)
    Result::Final(player: 7, points: 1904)
- 0m09 with ttl=2 on decisions

20 players
- 2m06
Result::Final(player: 0, points: 3792)
Result::Final(player: 1, points: 3923)
Result::Final(player: 2, points: 3582)
Result::Final(player: 3, points: 3766)
Result::Final(player: 4, points: 2883)
Result::Final(player: 5, points: 3115)
Result::Final(player: 6, points: 3458)
Result::Final(player: 7, points: 3523)
Result::Final(player: 8, points: 3360)
Result::Final(player: 9, points: 3067)
Result::Final(player: 10, points: 3486)
Result::Final(player: 11, points: 3363)
Result::Final(player: 12, points: 3582)
Result::Final(player: 13, points: 2947)
Result::Final(player: 14, points: 3706)
Result::Final(player: 15, points: 3739)
Result::Final(player: 16, points: 3435)
Result::Final(player: 17, points: 2931)
Result::Final(player: 18, points: 3690)
Result::Final(player: 19, points: 3640)


