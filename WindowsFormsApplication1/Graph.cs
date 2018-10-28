using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsFormsApplication1
{
    class Graph
    {
        private float[][] adjMatrix;
        private Dictionary<string, int> vertex;
        private Dictionary<int, string> reversVertex;

        public Graph()
        {
            vertex = new Dictionary<string, int>();
            reversVertex = new Dictionary<int, string>();
            adjMatrix = new float[0][];
        }

        public void AddVertex(string V)
        {
            //Console.WriteLine("Adding vertex " + V);

            if (vertex.ContainsKey(V))
            {
                //Console.WriteLine("The element already exists");
            }
            else
            {
                vertex.Add(V, adjMatrix.Length);
                reversVertex.Add(adjMatrix.Length, V);

                Array.Resize(ref adjMatrix, adjMatrix.Length + 1);
                adjMatrix[adjMatrix.Length - 1] = new float[adjMatrix.Length];
                for (int i = 0; i < adjMatrix.Length; i++)
                    Array.Resize(ref adjMatrix[i], adjMatrix.Length);
            }
        }

        public void RemoveVertex(string V)
        {
            //Console.WriteLine("\nRemoving vertex " + V);

            int value = 0;
            if (vertex.ContainsKey(V))
                value = vertex[V];

            if (!vertex.Remove(V))
            {
                //Console.WriteLine("The element doesnt exist");
                return;
            }

            for (int i = value + 1; i < reversVertex.Count; i++)
                vertex[reversVertex[i]] = i - 1;

            MakeReversDictionary();

            Array.Resize(ref adjMatrix, adjMatrix.Length - 1);
            adjMatrix[adjMatrix.Length - 1] = new float[adjMatrix.Length];
            for (int i = 0; i < adjMatrix.Length; i++)
                Array.Resize(ref adjMatrix[i], adjMatrix.Length);
        }

        private void MakeReversDictionary()
        {
            reversVertex.Clear();
            int j = 0;
            foreach (string i in vertex.Keys)
            {
                reversVertex.Add(j, i);
                j++;
            }
        }

        public void AddEdge(string A, string B, float w)
        {
            //Console.WriteLine("\nAdding edge " + A + B + " with weight " + w);

            if (A == B || !vertex.ContainsKey(A) || !vertex.ContainsKey(B) || w == 0)
            {
                //Console.WriteLine("\nnet");
                return;
            }

            adjMatrix[vertex[A]][vertex[B]] = w;
            adjMatrix[vertex[B]][vertex[A]] = w;
        }

        public void RemoveEdge(string A, string B)
        {
            //Console.WriteLine("\nRemoving edge " + A + B);

            adjMatrix[vertex[A]][vertex[B]] = 0;
            //Console.WriteLine("the edge has been deleted");
        }

        public T Adjacency<T>(string A, string B)
        {

            if (A == B || !vertex.ContainsKey(A) || !vertex.ContainsKey(B))
            {
                //Console.WriteLine("\nfalse");

                int i = 0;
                return (T)Convert.ChangeType(i, typeof(T));
            }
            else return (T)Convert.ChangeType(adjMatrix[vertex[A]][vertex[B]], typeof(T));
        }

        public List<string> Neibours(string A)
        {
            //Console.Write("\nNeibours of " + A + ": ");
            List<string> neibour = new List<string>();

            if (!vertex.ContainsKey(A)) return neibour;

            for (int i = 0; i < adjMatrix.Length; i++)
            {
                if (adjMatrix[vertex[A]][i] != 0) neibour.Add(reversVertex[i]);
            }
            return neibour;
        }

        public void PrintMatrix()
        {
            Console.Write("\n" + " ");

            foreach (string i in vertex.Keys)
                Console.Write(" " + i);

            Console.WriteLine();

            for (int i = 0; i < adjMatrix.Length; i++)
            {
                Console.Write(reversVertex[i]);

                for (int j = 0; j < adjMatrix.Length; j++)
                    Console.Write(" " + adjMatrix[i][j]);
                Console.WriteLine();
            }
        }

        public List<string> VertexList()
        {
            List<string> vertexList = new List<string>();
            foreach (string i in vertex.Keys)
            {
                vertexList.Add(i);
            }

            return vertexList;
        }

        public List<string> WideWidthSearch(string start, string goal, out float distance)
        {
            Dictionary<string, string> ways = new Dictionary<string, string>();
            List<string> visitedVertex = new List<string>();
            Queue<string> seach = new Queue<string>();
            List<string> temp;
            distance = 0;

            ways.Add(start, start); // present / last
            seach.Enqueue(start);

            while (!visitedVertex.Contains(goal))
            {
                temp = Neibours(seach.Peek());
                visitedVertex.Add(seach.Peek());

                foreach (string i in temp)
                {
                    if (visitedVertex.Contains(i)) continue;
                    if (!ways.ContainsKey(i))
                    {
                        ways.Add(i, seach.Peek());
                        seach.Enqueue(i);
                    }
                }
                seach.Dequeue();
            }
            seach.Clear();

            temp = new List<string>();
            temp.Add(ways[goal]);
            int j = 0;

            while (!temp.Contains(start))
            {
                temp.Add(ways[temp[j]]);
                j++;
                distance += adjMatrix[j][j - 1];
            }
            temp.Reverse();

            return temp;
        }

        public List<string> Dijkstar()
        {
            Dictionary<string, string> way = new Dictionary<string, string>(); // present / last
            Dictionary<string, float> wayLenght = new Dictionary<string, float>();
            Dictionary<string, float> seach = new Dictionary<string, float>();

            seach.Add(vertex.First().Key, 0f);
            wayLenght.Add(vertex.First().Key, 0f);

            while (seach.Count != 0)
            {
                var temp = seach.First(s => s.Value == seach.Values.Min()).Key;

                foreach (string i in Neibours(temp))
                {
                    var value = Adjacency<float>(temp, i) + wayLenght[temp];
                    if (!wayLenght.ContainsKey(i))
                    {
                        seach.Add(i, value);
                        wayLenght.Add(i, value);
                        way.Add(i, temp);
                    }
                    else
                    {
                        if (value <= wayLenght[i])
                        {
                            seach[i] = value;
                            wayLenght[i] = value;
                            way[i] = temp;
                        }
                    }
                }
                seach.Remove(temp);
            }

            List<string> temp_ = new List<string>();
            temp_.Add(vertex.First(s => s.Value == vertex.Values.Max()).Key);
            int j = 0;

            while (!temp_.Contains(reversVertex[0]))
            {
                temp_.Add(way[temp_[j]]);
                j++;
            }

            return temp_;
        }
    }
}
