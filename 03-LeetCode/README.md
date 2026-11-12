# Part C — LeetCode: Binary Search

**LeetCode Profile:** [Mostafa Abdella](https://leetcode.com/u/Mostafa_Abdella/)

This section contains two LeetCode problems solved in C# using Binary Search.

Both solutions were submitted successfully and received Accepted status.

## 1. Koko Eating Bananas

- **Problem:** [875. Koko Eating Bananas](https://leetcode.com/problems/koko-eating-bananas/)
- **Accepted Submission:** [View Submission](https://leetcode.com/problems/koko-eating-bananas/submissions/2192927210/)
- **Solution File:** `KokoEatingBananas.cs`
- **Screenshot:** [koko-accepted.png](screenshots/koko-accepted.png)

### Approach

I used Binary Search on the answer to find the minimum eating speed that allows Koko to finish all banana piles within `h` hours.

- **Low:** `1`, because Koko must eat at least one banana per hour.
- **High:** The maximum pile size, because eating at that speed guarantees finishing each pile in one hour.

For each middle speed (`mid`), I calculate the total required hours using:

```csharp
hours += (pile + mid - 1) / mid;
```

This calculates the ceiling of `pile / mid` using integer arithmetic.

**Decision:**

- If `hours <= h`, the current speed is sufficient, so I search the lower half by setting `high = mid`.
- Otherwise, the speed is too slow, so I search the upper half by setting `low = mid + 1`.

The loop ends when `low == high`. At this point, both boundaries represent the minimum valid eating speed.

### Example 1 — Manual Trace

**Input:**

```text
piles = [3, 6, 7, 11]
h = 8
```

| Step | Low | Mid | High | Required Hours | Decision |
|---|---:|---:|---:|---:|---|
| 1 | 1 | 6 | 11 | 6 | Sufficient → high = 6 |
| 2 | 1 | 3 | 6 | 10 | Too slow → low = 4 |
| 3 | 4 | 5 | 6 | 8 | Sufficient → high = 5 |
| 4 | 4 | 4 | 5 | 8 | Sufficient → high = 4 |

**Result:** `4`

### Complexity Analysis

- **Time Complexity:** `O(n * log(m))`, where `n` is the number of piles and `m` is the maximum pile size.
- **Space Complexity:** `O(1)` auxiliary space.

---

## 2. Search in Rotated Sorted Array

- **Problem:** [33. Search in Rotated Sorted Array](https://leetcode.com/problems/search-in-rotated-sorted-array/)
- **Accepted Submission:** [View Submission](https://leetcode.com/problems/search-in-rotated-sorted-array/submissions/2192926473/)
- **Solution File:** `SearchInRotatedSortedArray.cs`
- **Screenshot:** [rotated-accepted.png](screenshots/rotated-accepted.png)

### Approach

I used Modified Binary Search to find the target in a rotated sorted array in `O(log n)` time.

At every iteration, at least one half of the current search range is sorted.

- First, calculate `mid` and check whether `nums[mid]` equals the target.
- If `nums[low] <= nums[mid]`, the left half is sorted.
- Check whether the target belongs to the sorted left half. If it does, move `high` to `mid - 1`; otherwise, move `low` to `mid + 1`.
- If the left half is not sorted, the right half must be sorted.
- Check whether the target belongs to the sorted right half. If it does, move `low` to `mid + 1`; otherwise, move `high` to `mid - 1`.

If the search range becomes empty, the target does not exist, and the method returns `-1`.

### Example 1 — Manual Trace

**Input:**

```text
nums = [4, 5, 6, 7, 0, 1, 2]
target = 0
```

| Step | Low | Mid | High | Decision |
|---|---:|---:|---:|---|
| 1 | 0 | 3 | 6 | Left half is sorted; target is not in it → low = 4 |
| 2 | 4 | 5 | 6 | Left half [0, 1] is sorted; target is in it → high = 4 |
| 3 | 4 | 4 | 4 | Found target at index 4 |

**Result:** `4`

### Complexity Analysis

- **Time Complexity:** `O(log n)`, because the search range is reduced by approximately half in every iteration.
- **Space Complexity:** `O(1)` auxiliary space.

---

## Submission Summary

| Problem | Language | Algorithm | Status |
|---|---|---|---|
| 875. Koko Eating Bananas | C# | Binary Search on Answer | Accepted |
| 33. Search in Rotated Sorted Array | C# | Modified Binary Search | Accepted |

Both problems were solved without sorting the input, scanning all possible answers, or using built-in search methods.