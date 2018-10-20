using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace WindowsFormsApplication1
{
	public class Graph
	{
		private SortedDictionary<string, int> verticies = new SortedDictionary<string, int>();

		private int[][] adjecencyMatrix = new int[0][];

		public void AddVertex(string vertex)
		{
			if (verticies.ContainsKey(vertex))
			{
				Console.WriteLine("Vertex already exists");
				return;
			}

			verticies.Add(vertex, verticies.Count);
			Array.Resize(ref adjecencyMatrix, verticies.Count);
			for (int i = 0; i < verticies.Count; i++)
			{
				Array.Resize(ref adjecencyMatrix[i], verticies.Count);
			}
			adjecencyMatrix[verticies.Count - 1] = new int[verticies.Count];
			//Console.WriteLine("Add {0} : {1}", vertex, verticies[vertex]);
		}

		public void RemoveVertex(string vertex)
		{
			if (!verticies.ContainsKey(vertex))
			{
				Console.WriteLine("No such vertex");
			}
			else
			{
				//Console.WriteLine("Remove {0}", vertex);
				for (int i = 0; i < verticies.Count; i++)
				{
					RemoveAt(ref adjecencyMatrix[i], verticies[vertex]);
				}

				for (int i = 0; i < verticies.Count; i++)
				{
					if (verticies.ElementAt(i).Value > verticies[vertex])
					{
						verticies[verticies.ElementAt(i).Key]--;
					}
				}
				verticies.Remove(vertex);
			}
		}

		public static void RemoveAt<T>(ref T[] arr, int index)
		{
			for (int a = index; a < arr.Length - 1; a++)
			{
				arr[a] = arr[a + 1];
			}
			Array.Resize(ref arr, arr.Length - 1);
		}

		public void AddEdge(string vertex1, string vertex2, int weight = 1)
		{
			if (!verticies.ContainsKey(vertex1) || !verticies.ContainsKey(vertex2))
			{
				Console.WriteLine("Vertex doesn't exist");
				return;
			}

			adjecencyMatrix[verticies[vertex1]][verticies[vertex2]] = weight;
			adjecencyMatrix[verticies[vertex2]][verticies[vertex1]] = weight;
		}

		public void RemoveEdge(string vertex1, string vertex2)
		{
			AddEdge(vertex1, vertex2, 0);
		}

		public T GetAdjecency<T>(string vertex1, string vertex2)
		{
			if (!verticies.ContainsKey(vertex1) || !verticies.ContainsKey(vertex2))
			{
				Console.WriteLine("Vertex doesn't exist");
				return default(T);
			}

			return (T)Convert.ChangeType(adjecencyMatrix[verticies[vertex1]][verticies[vertex2]], typeof(T));
		}

		public List<string> GetNeighbours(string vertex)
		{
			if (!verticies.ContainsKey(vertex))
			{
				Console.WriteLine("Vertex doesn't exist");
				return new List<string>();
			}

			var output = new List<string>();
			var inverseDictionary = GetInverseVertexDictionary();
			for (int i = 0; i < verticies.Count; i++)
			{
				if (adjecencyMatrix[verticies[vertex]][i] != 0)
				{
					output.Add(inverseDictionary[i]);
				}
			}
			//Console.WriteLine("Neighbours of {0}:", vertex);
			//output.ForEach(Console.WriteLine);
			//Console.WriteLine();
			return output;
		}

		public void PrintMatrix()
		{
			var inverseDictionary = GetInverseVertexDictionary();
			Console.Write(" \t");
			for (int i = 0; i < verticies.Count; i++)
			{
				Console.Write(inverseDictionary[i] + "\t");
			}
			Console.WriteLine();
			for (int i = 0; i < verticies.Count; i++)
			{
				Console.Write(inverseDictionary[i] + "\t");
				for (int j = 0; j < verticies.Count; j++)
				{
					Console.Write("{0}\t", adjecencyMatrix[i][j]);
				}
				Console.WriteLine();
			}
		} //1=есть связь, 0=нет связи

		public void PrintVerticies()
		{
			foreach (var kvp in verticies)
			{
				Console.WriteLine(kvp);
			}
		}

		private Dictionary<int, string> GetInverseVertexDictionary()
		{
			var output = new Dictionary<int, string>();
			foreach (var kvp in verticies)
			{
				output[kvp.Value] = kvp.Key;
			}

			return output;
		}
	}
}

