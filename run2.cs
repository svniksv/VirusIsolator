using System;
using System.Collections.Generic;
using System.Linq;

class Program
{

    static List<string> Solve(List<(string, string)> edges)
    {
        Graph graph = new Graph(edges);
        bool first = true;

        while (!graph.IsIsolated())
        {
            BFS(ref graph, graph.Virus, ref first);
        }

        return graph.Determination;
    }

    static void BFS(ref Graph graph, int start, ref bool first)
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
                if (distances[connection] == -1 || distances[current] + 1 < distances[connection]) distances[connection] = distances[current] + 1; // заполняем массив длин
            }
        }
        //ищем ближайший шлюз
        int end = Array.FindIndex(graph.Nodes, (node) => node.All(char.IsUpper)); // считаем, что искомый шлюз - А
        while (distances[end] == -1) end++; //если выбранный шлюз уже отключен, берём следующий.
        if (end != graph.Nodes.Length - 1) // если осталось больше 1 шлюза, то ищем кратчайший или лексикографически меньший
        {
            for (int i = end + 1; i < graph.Nodes.Length; i++)
                if (distances[end] > distances[i] && distances[i] != -1) end = i;
        }

        
        List<string> allShortPath = RecreatePath(graph, distances, end);
        //реверсируем и сортируем все пути, чтобы найти лексикографически меньший
        for (int i = 0; i < allShortPath.Count; i++)
            allShortPath[i] = new string(allShortPath[i].ToCharArray().Reverse().ToArray());

        //сортируем массив
        allShortPath.Sort((path1, path2) =>
        {
            var nodes1 = path1.Split('-');
            var nodes2 = path2.Split('-');

            for (int i = 0; i < path1.Length; i++)
            {
                if (nodes1[i].Length != nodes2[i].Length) return nodes1[i].Length.CompareTo(nodes2[i].Length);
                int comparison = string.Compare(nodes1[i], nodes2[i]);
                if (comparison != 0) return comparison;
            }
            return 0;
        });

        //Console.WriteLine("Path: " + allShortPath[0]);
        var firstPath = allShortPath[0].Split("-");
        //двигаем вирус в каждой итерации кроме первой 
        if (!first) graph.Virus = Array.IndexOf(graph.Nodes, firstPath[1]);
        else first = false;
        //Console.WriteLine("Virus in " + graph.Nodes[graph.Virus]);

        //отключение шлюза
        List<string> gatewayConnections= new List<string>();
        foreach (var p in allShortPath)
        {
            var nodes = p.Split("-");
            gatewayConnections.Add(nodes[nodes.Length - 2]);
        }

        gatewayConnections.Sort((x, y) =>
        {
            if (x.Length != y.Length) return x.Length.CompareTo(y.Length);
            int minLength = Math.Min(x.Length, y.Length);
            for (int i = 0; i < minLength; i++)
            {
                if (x[i] != y[i])
                    return x[i].CompareTo(y[i]);
            }
            return x.Length.CompareTo(y.Length);
        });

        if (graph.GraphMatrix[graph.Virus, end] == 1) graph.Determination.Add(graph.RemoveConnection((end, graph.Virus)));
        else graph.Determination.Add(graph.RemoveConnection((end, Array.IndexOf(graph.Nodes, gatewayConnections[0]))));
        
    }

    //получаем все возмжные кратчайшие пути до шлюза
    static List<string> RecreatePath(Graph graph, int[] dist, int end)
    {
        List<string> path = new List<string>();

        for (int i = 0; i < dist.Length; i++)
        {
            if (dist[i] == dist[end] - 1 && graph.GraphMatrix[i, end] == 1)
            {
                if (dist[i] == dist[graph.Virus])
                {
                    path.Add(graph.Nodes[end] + "-" + graph.Nodes[i]);
                    return path;
                }
                var next = RecreatePath(graph, dist, i);
                foreach (var n in next) path.Add(graph.Nodes[end] + "-" + n);
            }
        }

        return path;
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
    public List<string> Determination = new List<string>();

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
        return (Nodes[edges.Item1] + "-" + Nodes[edges.Item2]);
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

        for (int i = 0; i < Nodes.Length; i++)
            if (GraphMatrix[index, i] != 0) connections.Add(i);
        return connections;
    }

    private string[] CreateNodeList(List<(string, string)> nodes)
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

        knots.Sort((x, y) =>
        {
            if (x.Length != y.Length) return x.Length.CompareTo(y.Length);
            int minLength = Math.Min(x.Length, y.Length);
            for (int i = 0; i < minLength; i++)
            {
                if (x[i] != y[i])
                    return x[i].CompareTo(y[i]);
            }
            return x.Length.CompareTo(y.Length);
        });

        gateway.Sort((x, y) =>
        {
            if (x.Length != y.Length) return x.Length.CompareTo(y.Length);
            int minLength = Math.Min(x.Length, y.Length);
            for (int i = 0; i < minLength; i++)
            {
                if (x[i] != y[i])
                    return x[i].CompareTo(y[i]);
            }
            return x.Length.CompareTo(y.Length);
        });

        List<string> totalNodes = knots.Concat(gateway).ToList();

        string[] result = new string[totalNodes.Count];
        for (int i = 0; i < totalNodes.Count; i++) result[i] = totalNodes[i];
        return result;
    }
}