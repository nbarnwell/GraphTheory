using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using NUnit.Framework.Interfaces;

namespace GraphTraversal.Tests.Unit
{
    public static class Relationships
    {
        public const string IsOrbitedBy = "isOrbitedBy";
    }

    [TestFixture]
    public class Class1
    {
        [Test]
        public void Create_a_graph()
        {
            var centreOfUniverse = new CentreOfUniverse("Universe");

            var sol = new Star("Sol");
            centreOfUniverse.AddRelationship(Relationships.IsOrbitedBy, sol);

            var venus = new Planet("Venus");
            sol.AddRelationship(Relationships.IsOrbitedBy, venus);

            var searcher = new DepthFirstGraphSearch(centreOfUniverse);

            var foundPlanet = searcher.Search(x => x.Id == "Venus");

            Assert.AreEqual(venus.Id, foundPlanet.Single().Id);
        }
    }

    public class DepthFirstGraphSearch
    {
        private readonly Node _start;

        public DepthFirstGraphSearch(Node start)
        {
            _start = start ?? throw new ArgumentNullException(nameof(start));
        }

        public IEnumerable<Node> Search(Predicate<Node> filter)
        {
            return SearchCore(_start, filter, new HashSet<string>());
        }

        private IEnumerable<Node> SearchCore(Node start, Predicate<Node> filter, ISet<string> visited)
        {
            if (filter(start))
            {
                yield return start;
            }
            else
            {
                foreach (var relationship in start.Relationships)
                {
                    if (!visited.Contains(relationship.Relation.Id))
                    {
                        var relatives = SearchCore(relationship.Relation, filter, visited);
                        foreach (var relative in relatives)
                        {
                            yield return relative;
                        }

                        visited.Add(relationship.Id);
                    }
                }
            }
        }
    }

    public class CentreOfUniverse : AstronomicalObject
    {
        public CentreOfUniverse(string name) : base(name, name)
        {
        }
    }

    public class Star : AstronomicalObject
    {
        public Star(string name) : base(name, name)
        {
        }
    }

    public abstract class AstronomicalObject : Node
    {
        public string Name { get; }

        protected AstronomicalObject(string id, string name) : base(id)
        {
            Name = name;
        }
    }

    public class Planet : AstronomicalObject
    {
        public Planet(string name) : base(name, name)
        {
        }
    }

    public class Relationship
    {
        public string Kind { get; }
        public Node Relation { get; }
        public string Id => $"{Kind}-{Relation.Id}";

        public Relationship(string kind, Node relation)
        {
            Kind = kind ?? throw new ArgumentNullException(nameof(kind));
            Relation = relation ?? throw new ArgumentNullException(nameof(relation));
        }
    }

    public abstract class Node
    {
        private readonly IDictionary<string, IDictionary<string, Relationship>> _relationships =
            new Dictionary<string, IDictionary<string, Relationship>>();

        public IEnumerable<Relationship> Relationships
        {
            get { return _relationships.Values.SelectMany(x => x.Values); }
        }

        public string Id { get; }

        protected Node(string id)
        {
            Id = id;
        }

        public void AddRelationship(string kind, Node relation)
        {
            if (!_relationships.TryGetValue(kind, out var nodes))
            {
                nodes = new Dictionary<string, Relationship>();
                _relationships.Add(kind, nodes);
            }

            if (!nodes.ContainsKey(relation.Id))
            {
                nodes.Add(relation.Id, new Relationship(kind, relation));
            }
        }
    }
}
