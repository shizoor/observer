//Demonstration of the observer design pattern
namespace observerdemo;

using System.Collections.Generic;
using static observerdemo.Program;

public static class Program
{

    class Subject
    {
        public string state = "";
        public void SetState(string s) { state = s; }
        public List<Observer> observers = new List<Observer>();
        public void AttachObserver(Observer observer) { observers.Add(observer); }
        public void RemoveObserver(Observer observer) { observers.Remove(observer); }
        protected virtual string FromName => GetType().Name;
        public void NotifyAllObservers()
        {
            foreach (var observer in observers)
            {
                observer.Update(state, FromName);
            }
        }
    }
    class Machine : Subject
    {
        public string name = "";
        protected override string FromName => name;
    }

    class Observer   // "string from" is the machine's name.
    {
        public string name = "";
        public string lastState = "";
        public virtual void Update(string state, string from)
        {
            lastState = state;
        }



    }
    class Employee : Observer
    {
        public string role = "";
        public override void Update(string state, string from)
        {
            base.Update(state, from);
            Console.WriteLine(name + " who is a " + role + " saw " + from + " change to " + state);
        }

    }
    public static void Main(string[] args)
    {
        Employee e = new Employee();
        Employee f = new Employee();
        f.role = "technician";
        f.name = "Ian";
        e.role = "tech";
        e.name = "Douglas";
        Machine m = new Machine();
        m.name = "soldermachine1";
        m.AttachObserver(e);
        m.SetState("starved");
        m.NotifyAllObservers();
        m.SetState("producing");
        m.AttachObserver(f);
        m.NotifyAllObservers();
        m.SetState("idle");
        m.NotifyAllObservers();
        Console.WriteLine("finished");
        m.RemoveObserver(e);
        m.NotifyAllObservers();

    }
}



