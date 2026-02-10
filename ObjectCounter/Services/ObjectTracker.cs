using System;
using System.Collections.Generic;
using System.Linq;
using ObjectCounter.Models;

namespace ObjectCounter.Services
{
    public class ObjectTracker
    {
        private int _nextId = 0;
        private Dictionary<int, (float X, float Y)> _objects = new Dictionary<int, (float, float)>();
        private Dictionary<int, int> _disappeared = new Dictionary<int, int>();
        private const int MaxDisappeared = 20;
        private const float MaxDistance = 150.0f; 

        public List<DetectionResult> Update(List<DetectionResult> rects)
        {
            if (rects.Count == 0)
            {
                foreach (var id in _objects.Keys.ToList())
                {
                    _disappeared[id]++;
                    if (_disappeared[id] > MaxDisappeared)
                    {
                        Unregister(id);
                    }
                }
                return rects;
            }

            var inputCentroids = rects.Select(r => (X: r.X + r.Width / 2, Y: r.Y + r.Height / 2)).ToList();

            if (_objects.Count == 0)
            {
                foreach (var rect in rects)
                {
                    Register(rect, (rect.X + rect.Width / 2, rect.Y + rect.Height / 2));
                }
                return rects;
            }

            var objectIds = _objects.Keys.ToList();
            var objectCentroids = _objects.Values.ToList();

            // Calculate distance matrix
            // Simple greedy
            var usedRows = new HashSet<int>();
            var usedCols = new HashSet<int>();

            // Calculate all pair distances
            var distances = new List<(int ObjIdx, int InputIdx, float Dist)>();
            for (int i = 0; i < objectIds.Count; i++)
            {
                for (int j = 0; j < inputCentroids.Count; j++)
                {
                    float dx = objectCentroids[i].X - inputCentroids[j].X;
                    float dy = objectCentroids[i].Y - inputCentroids[j].Y;
                    float dist = (float)Math.Sqrt(dx*dx + dy*dy);
                    distances.Add((i, j, dist));
                }
            }

            // Sort by distance
            distances.Sort((a, b) => a.Dist.CompareTo(b.Dist));

            foreach (var (r, c, d) in distances)
            {
                if (usedRows.Contains(r) || usedCols.Contains(c)) continue;
                if (d > MaxDistance) continue;

                int objectId = objectIds[r];
                _objects[objectId] = inputCentroids[c];
                _disappeared[objectId] = 0;
                
                rects[c].TrackId = objectId;

                usedRows.Add(r);
                usedCols.Add(c);
            }

            // Unused rows (lost objects)
            var unusedRows = Enumerable.Range(0, objectIds.Count).Where(x => !usedRows.Contains(x));
            foreach (var row in unusedRows)
            {
                int objectId = objectIds[row];
                _disappeared[objectId]++;
                if (_disappeared[objectId] > MaxDisappeared)
                {
                    Unregister(objectId);
                }
            }

            // Unused cols (new objects)
            var unusedCols = Enumerable.Range(0, inputCentroids.Count).Where(x => !usedCols.Contains(x));
            foreach (var col in unusedCols)
            {
                Register(rects[col], inputCentroids[col]);
            }

            return rects;
        }

        private void Register(DetectionResult rect, (float X, float Y) centroid)
        {
            _objects[_nextId] = centroid;
            _disappeared[_nextId] = 0;
            rect.TrackId = _nextId;
            _nextId++;
        }

        private void Unregister(int id)
        {
            _objects.Remove(id);
            _disappeared.Remove(id);
        }
    }
}
