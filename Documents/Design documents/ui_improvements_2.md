# Storage Initialization (Step 3) — Specification

Improvements for current Initialization wizard (Step 3)
- Initialization errors should be shown at the top of the wizard.
- Shelf should show content like table:

ContainerTypeGroupRow | number1 | number2 | number3 | .... | number[i].max |
                      | number1 | number2 | number3 | .... | number[i].max |

Example:
Starting index: 1000
- Example for Shelf 1:
  - Group row columns count: 3
  - Group row 1: PX12
  - Group row 2: PX6

Should be presented as a table

| PX12 | 1000 | 1002 | .... | 1011 |
|      | 1012 | 1013 | .... | 1023 |
|      | 1024 | 1025 | .... | 1035 |
| PX6  | 1036 | 1037 | .... | 1041 |
| PX6  | 1042 | 1043 | .... | 1047 |
| PX6  | 1048 | 1049 | .... | 1053 |
