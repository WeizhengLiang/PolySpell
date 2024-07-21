// using System;
// using UnityEngine;
// using System.Collections.Generic;
//
// public static class SweepLineIntersection
// {
//     public static bool CheckIfPolygon(List<Vector2> segmentStartPositions, List<Vector2> segmentEndPositions)
//     {
//         var events = new List<Event>();
//
//         for (int i = 0; i < segmentStartPositions.Count; i++)
//         {
//             events.Add(new Event(segmentStartPositions[i], true, i));
//             events.Add(new Event(segmentEndPositions[i], false, i));
//         }
//
//         events.Sort();
//
//         var activeSegments = new SortedSet<int>(new SegmentComparer(segmentStartPositions, segmentEndPositions));
//
//         for (int i = 0; i < events.Count; i++)
//         {
//             var e = events[i];
//
//             if (e.isStart)
//             {
//                 activeSegments.Add(e.index);
//
//                 var prev = GetPrevious(activeSegments, e.index);
//                 var next = GetNext(activeSegments, e.index);
//
//                 if (prev != -1 && LineSegmentsIntersect(segmentStartPositions[e.index], segmentEndPositions[e.index], segmentStartPositions[prev], segmentEndPositions[prev], out _))
//                     return true;
//                 if (next != -1 && LineSegmentsIntersect(segmentStartPositions[e.index], segmentEndPositions[e.index], segmentStartPositions[next], segmentEndPositions[next], out _))
//                     return true;
//             }
//             else
//             {
//                 var prev = GetPrevious(activeSegments, e.index);
//                 var next = GetNext(activeSegments, e.index);
//
//                 activeSegments.Remove(e.index);
//
//                 if (prev != -1 && next != -1 && LineSegmentsIntersect(segmentStartPositions[prev], segmentEndPositions[prev], segmentStartPositions[next], segmentEndPositions[next], out _))
//                     return true;
//             }
//         }
//
//         return false;
//     }
//
//     private static int GetPrevious(SortedSet<int> set, int current)
//     {
//         int prev = -1;
//         foreach (var item in set)
//         {
//             if (item == current) return prev;
//             prev = item;
//         }
//         return -1;
//     }
//
//     private static int GetNext(SortedSet<int> set, int current)
//     {
//         bool found = false;
//         foreach (var item in set)
//         {
//             if (found) return item;
//             if (item == current) found = true;
//         }
//         return -1;
//     }
//
//     public static bool LineSegmentsIntersect(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2, out Vector2 intersection)
//     {
//         intersection = Vector2.zero;
//
//         float a1 = q1.y - p1.y;
//         float b1 = p1.x - q1.x;
//         float c1 = a1 * p1.x + b1 * p1.y;
//
//         float a2 = q2.y - p2.y;
//         float b2 = p2.x - q2.x;
//         float c2 = a2 * p2.x + b2 * p2.y;
//
//         float determinant = a1 * b2 - a2 * b1;
//
//         if (determinant == 0)
//         {
//             return false; // Parallel lines
//         }
//         else
//         {
//             float x = (b2 * c1 - b1 * c2) / determinant;
//             float y = (a1 * c2 - a2 * c1) / determinant;
//             intersection = new Vector2(x, y);
//
//             if (IsPointOnLineSegment(p1, q1, intersection) && IsPointOnLineSegment(p2, q2, intersection))
//             {
//                 return true;
//             }
//         }
//
//         return false;
//     }
//
//     public static bool IsPointOnLineSegment(Vector2 p, Vector2 q, Vector2 r)
//     {
//         return (r.x <= Mathf.Max(p.x, q.x) && r.x >= Mathf.Min(p.x, q.x) && r.y <= Mathf.Max(p.y, q.y) && r.y >= Mathf.Min(p.y, q.y));
//     }
// }
//
// public class Event : IComparable<Event>
// {
//     public Vector2 point;
//     public bool isStart;
//     public int index;
//
//     public Event(Vector2 point, bool isStart, int index)
//     {
//         this.point = point;
//         this.isStart = isStart;
//         this.index = index;
//     }
//
//     public int CompareTo(Event other)
//     {
//         int result = point.x.CompareTo(other.point.x);
//         if (result == 0)
//         {
//             result = point.y.CompareTo(other.point.y);
//             if (result == 0)
//                 result = isStart.CompareTo(other.isStart);
//         }
//         return result;
//     }
// }
//
// public class SegmentComparer : IComparer<int>
// {
//     private List<Vector2> segmentStartPositions;
//     private List<Vector2> segmentEndPositions;
//
//     public SegmentComparer(List<Vector2> segmentStartPositions, List<Vector2> segmentEndPositions)
//     {
//         this.segmentStartPositions = segmentStartPositions;
//         this.segmentEndPositions = segmentEndPositions;
//     }
//
//     public int Compare(int i, int j)
//     {
//         var si = segmentStartPositions[i];
//         var ei = segmentEndPositions[i];
//         var sj = segmentStartPositions[j];
//         var ej = segmentEndPositions[j];
//
//         if (si.x < sj.x && ei.x < sj.x)
//             return -1;
//         if (si.x > ej.x && ei.x > ej.x)
//             return 1;
//
//         float yi = si.y + (ei.y - si.y) * (sj.x - si.x) / (ei.x - si.x);
//         float yj = sj.y + (ej.y - sj.y) * (si.x - sj.x) / (ej.x - sj.x);
//
//         return yi.CompareTo(yj);
//     }
// }