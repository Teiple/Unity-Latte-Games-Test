using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Source code: https://github.com/Athenkosi007/Disjoint-Set-Union-Find- 
namespace DataStructures
{
    namespace UnionFind
    {
        class Node<T>
            where T : System.IComparable<T>
        {
            public T Data { get; set; }
            public Node<T> Parent { get; set; }
            public int Rank { get; set; }

            public Node(T data)
            {
                Data = data;
                Parent = this;
                Rank = 0;
            }
        }

        public class DisjointSet<T> : IEnumerable<T>
            where T : System.IComparable<T>
        {
            Dictionary<T, Node<T>> nodes;

            public int Count { get { return nodes.Count; } }

            public DisjointSet()
            {
                nodes = new Dictionary<T, Node<T>>();
            }

            public IEnumerator<T> GetEnumerator()
            {
                return nodes.Keys.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return nodes.Keys.GetEnumerator();
            }

            public bool ContainsData(T data)
            {
                return nodes.ContainsKey(data);
            }

            public bool MakeSet(T data)
            {
                if (ContainsData(data))
                    return false;

                nodes.Add(data, new Node<T>(data));
                return true;
            }

            public bool Union(T dataA, T dataB)
            {
                var nodeA = nodes[dataA];
                var nodeB = nodes[dataB];

                var parentA = nodeA.Parent;
                var parentB = nodeB.Parent;

                if (parentA == parentB)
                    return false;

                if (parentA.Rank >= parentB.Rank)
                {
                    if (parentA.Rank == parentB.Rank)
                        ++parentA.Rank;

                    parentB.Parent = parentA;
                }
                else
                {
                    parentA.Parent = parentB;
                }

                return true;
            }

            public T FindSet(T data)
            {
                return FindSet(nodes[data]).Data;
            }

            public bool IsEmpty()
            {
                return Count == 0;
            }

            public void Clear()
            {
                nodes.Clear();
            }

            Node<T> FindSet(Node<T> node)
            {
                var parent = node.Parent;
                if (parent == node)
                    return node;

                node.Parent = FindSet(node.Parent);
                return node.Parent;
            }

            // Added from me!
            public T[] GetAndRemoveNonDistinctElements()
            {
                var groups = new Dictionary<Node<T>, List<T>>();

                foreach (var kvp in nodes)
                {
                    var root = FindSet(kvp.Value);
                    if (!groups.ContainsKey(root))
                        groups[root] = new List<T>();

                    groups[root].Add(kvp.Key);
                }

                var result = new List<T>();
                foreach (var group in groups.Values)
                {
                    if (group.Count > 1)
                    {
                        result.AddRange(group);
                        foreach (var item in group)
                        {
                            nodes.Remove(item);
                        }
                    }
                }

                return result.ToArray();
            }

            public T[][] GetAllSets()
            {
                // Dictionary to group by root representative
                var groups = new Dictionary<Node<T>, List<T>>();

                foreach (var kvp in nodes)
                {
                    var root = FindSet(kvp.Value);
                    if (!groups.ContainsKey(root))
                        groups[root] = new List<T>();

                    groups[root].Add(kvp.Key);
                }

                // Convert each group to array, then all groups to jagged array
                return groups.Values.Select(g => g.ToArray()).ToArray();
            }

        }
    }
}
