# Bug List #1

## 1. Layout does not fit screen
- My screen resolution is `1920x1080`.
- The web application is running in Chrome and does not fit the screen. (too wide)

## 2. Burger menu structure
- The burger menu should contain the following sections:
  1. **Store initialization**
     - `Storage Init` button
  2. **Directory settings**
     - `Configure directories` button
  3. **Theme section**
     - Dark/Light theme switch

## 3. Storage initialization - Step 3
- `PX4` container group is rendered incorrectly.

Current rendering:
PX4 | 1666 | 1667 | 1668 | 1669
    | 1670 | 1671 | 1672 | 1673
    | 1674 | 1675 | 1676 | 1677

Correct rendering:
PX4 | 1666 | 1667 | 1668 | 1669 | 1670 | 1671
    | 1672 | 1673 | 1674 | 1675 | 1676 | 1677

## 4. File path selectors
- Should be a file picker, not a text box.
