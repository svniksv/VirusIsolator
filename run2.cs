using System;
using System.Collections.Generic;
using System.Linq;

class Program
{

    static List<string> Solve(List<(string, string)> edges)
    {
        var result = new List<string>();

        Graph graph = new Graph(edges);
        bool first = true;

        while (!graph.IsIsolated())
        {
            var currentPath = new List<(int, int)>();
            currentPath = BFS(graph, graph.Virus);
            if (!first) graph.Virus = currentPath.Last().Item1;
            else first = false;
            result.Add(graph.RemoveConnection(currentPath[0]));
        }

        return result;
    }

    static List<(int, int)> BFS(Graph graph, int start)
    {
        var visited = new HashSet<int>();
        var queue = new Queue<int>();

        int[] distances = new int[graph.Nodes.Length];
        for (int i = 0; i < distances.Length; i++) distances[i] = -1;
        distances[start] = 0;

        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue(); // берем элемент из очереди и помечаем его посещенным 
            visited.Add(current);

            List<int> connectedNodes = graph.GetConnectedNodes(current); // получаем всех соседей узла

            foreach (var connection in connectedNodes)
            {
                if (visited.Contains(connection)) continue; // пропускаем исследованных соседей

                queue.Enqueue(connection); //добавляем соседей в очередь
                if (distances[connection] == -1 || distances[current] + 1 < distances[connection]) distances[connection] = distances[current] + 1;
            }
        }
        //ищем ближайший шлюз
        int end = Array.FindIndex(graph.Nodes, (node) => node.All(char.IsUpper));
        while (distances[end] == -1) end++;
        if (end != graph.Nodes.Length - 1)
        {
            for (int i = end + 1; i < graph.Nodes.Length; i++)
                if (distances[end] > distances[i] && distances[i] != -1) end = i;
        }

        var shortestPath = new List<(int, int)>();
        while (end != start)
        {
            for (int i = 0; i < distances.Length; i++)
            {
                if (distances[i] == distances[end] - 1 && graph.GraphMatrix[i,end] == 1)
                {
                    shortestPath.Add((end, i));
                    end = i;
                    break;
                }
            }
        }
        return shortestPath;
    }

    static void Main()
    {
        var edges = new List<(string, string)>();
        string line;

        while ((line = Console.ReadLine()) != null)
        {
            line = line.Trim();
            if (!string.IsNullOrEmpty(line))
            {
                var parts = line.Split('-');
                if (parts.Length == 2)
                {
                    edges.Add((parts[0], parts[1]));
                }
            }
        }

        var result = Solve(edges);
        foreach (var edge in result)
        {
            Console.WriteLine(edge);
        }
    }
}

public class Graph
{
    public int[,] GraphMatrix;
    public string[] Nodes;
    public int Virus;

    public Graph(List<(string, string)> edges)
    {
        Nodes = CreateNodeList(edges);
        GraphMatrix = new int[Nodes.Length, Nodes.Length];
        Virus = 0;
        FillingGraph(edges);
    }

    private void FillingGraph(List<(string, string)> edges)
    {
        foreach (var edge in edges)
            AddConnection(edge);
    }

    private void AddConnection((string, string) edges)
    {
        int i = Array.IndexOf(Nodes, edges.Item1);
        int j = Array.IndexOf(Nodes, edges.Item2);
        GraphMatrix[i, j] = GraphMatrix[j, i] = 1;
    }

    public string RemoveConnection((int, int) edges)
    {
        GraphMatrix[edges.Item1, edges.Item2] = GraphMatrix[edges.Item2, edges.Item1] = 0;
        return (Nodes[edges.Item1] + "-" +  Nodes[edges.Item2]);
    }

    public bool IsIsolated()
    {
        for (int i = 0; i < Nodes.Length; i++)
        {
            if (Nodes[i].All(char.IsUpper))
            {
                for (int j = 0; j < Nodes.Length; j++)
                {
                    if (GraphMatrix[i, j] != 0) return false;
                }
            }
        }
        return true;
    }

    public List<int> GetConnectedNodes(int index)
    {
        List<int> connections = new List<int>();

        for (int i = 0;i < Nodes.Length;i++)
            if (GraphMatrix[index,i] != 0) connections.Add(i);
        return connections;
    }

    private string[] CreateNodeList(List<(string,string)> nodes)
    {
        List<string> knots = new List<string>();
        List<string> gateway = new List<string>();

        foreach (var item in nodes)
        {
            if (!(knots.Contains(item.Item1) || gateway.Contains(item.Item1)))
            {
                if (item.Item1.All(char.IsLower)) knots.Add(item.Item1);
                else gateway.Add(item.Item1);
            }
            if (!(knots.Contains(item.Item2) || gateway.Contains(item.Item2)))
            {
                if (item.Item2.All(char.IsLower)) knots.Add(item.Item2);
                else gateway.Add(item.Item2);
            }
        }

        knots.Sort();
        gateway.Sort();

        List<string> totalNodes = knots.Concat(gateway).ToList();

        string[] result = new string[totalNodes.Count];
        for (int i = 0; i < totalNodes.Count; i++) result[i] = totalNodes[i];
        return result;
    }
}