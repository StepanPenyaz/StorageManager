# Storage Management System Representation (UI)

## 1. Overview

A new web application should be created to provide a visual representation of the storage system.

---

## 2. General Requirements

* The application should be a Single Page Application (SPA)
* Each storage cabinet should be presented as a separate tab
* Each cabinet shelf should be visually separated with a 2 px border

---

## 3. Shelf Representation

* Each shelf should display **only containers with empty sections**

### Example

Database shelf layout:

```
 #1000 #1001 #1002 #1003
 #1004 #1005 #1006 #1007
 #1008 #1009 #1010 #1011
 #1012 #1013 #1014 #1015
```

Where `#1000 - #1015` are container numbers.

Containers with empty sections:

* `#1000` → 1 empty section
* `#1003` → 3 empty sections
* `#1010` → 2 empty sections

UI representation:

```
 #1000 #1003
 #1010
```

---

## 4. Container Representation

* Each container should be displayed as a rectangle
* Aspect ratio: **1:3 (height to width)**
* All container types should have the same dimensions

### Label

* Each rectangle should have a label at the top
* Format: `#1000`, `#1234` (prefix `#` is required)

---

## 5. Container Rendering Rules

### PX12 Container

* If the container has **1 empty section**:

  * 1/3 of the rectangle → green
  * 2/3 → red

* If the container has **2 empty sections**:

  * 2/3 → green
  * 1/3 → red

### PX12, PX6, PX4, PX2 Containers

* If the container has **all sections empty**:

  * Entire rectangle → green

---

## 6. Layout Requirements

* The application should be correctly rendered at resolution **1920x1080**
* The top **120 px** should be reserved for header elements

---

## 7. Notes

* Rendering should be consistent across all container types
* UI should clearly indicate available (empty) capacity