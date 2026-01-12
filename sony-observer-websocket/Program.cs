using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace sony_observer_websocket
{
    internal class Program
    {

        class Subject
        {
            public string state = "idle";
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

        class Observer : WebSocketBehavior   // "string from" is the machine's name.
        {
            protected static WebSocketSessionManager SessionHub;

            public string name = "";
            public string lastState = "";
            public virtual void Update(string state, string from)
            {
                lastState = state;
            }

            protected override void OnOpen()
            {
                // Store the session manager when a real connection opens
                if (SessionHub == null) { SessionHub = Sessions; }
                base.OnOpen();
            }

            protected override void OnMessage(MessageEventArgs e)
            {
                Console.WriteLine("Received message from EchoAll client : " + e.Data);
                Sessions.Broadcast(e.Data);
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

        class Dashbard : Observer
        {
            public override void Update(string state, string from)
            {
                base.Update(state, from);
                var msg = "{\n\"machinename\": \"" + from + "\",\n\"state\": \"" + state + "\"\n}";
                //SessionHub?.Broadcast(msg); Potentially cleaner but wanted a message when no client connected. 
                Console.WriteLine("name : " + name + "\n machine : " + from + "\n  changes to " + state);
                if (SessionHub != null)
                {
                    SessionHub.Broadcast(msg);
                }
                else
                {
                    Console.WriteLine("No websocket clients connected; message not sent.");
                }
            }

        }


        public static void Main(string[] args)
        {
            Random rnd = new Random();
            WebSocketServer wssv = new WebSocketServer("ws://127.0.0.1:7878");
            wssv.AddWebSocketService<Observer>("/Dashboard");
            wssv.Start();
            Console.WriteLine("Server started on port 7878");


            Dashbard d = new Dashbard();

            Machine a = new Machine();
            a.name = "test";

            Machine[] machines = new Machine[3];
            for (int i = 0; i < 3; i++)
            {
                machines[i] = new Machine();
            }
            machines[0].name = "Machine A";
            machines[0].AttachObserver(d);
            machines[1].name = "Machine B";
            machines[1].AttachObserver(d);
            machines[2].name = "Machine C";
            machines[2].AttachObserver(d);

            while (true)
            {
                for (int i = 0; i < 3; i++)
                {
                    int q = rnd.Next(0, 3);
                    switch (q)
                    {
                        case 0:
                            machines[i].state = "idle";
                            break;
                        case 1:
                            machines[i].state = "producing";
                            break;
                        case 2:
                            machines[i].state = "starved";
                            break;
                    }

                }
                for (int i = 0; i < 3; i++) machines[i].NotifyAllObservers();

                System.Console.WriteLine("sleeping for 1 second");
                System.Threading.Thread.Sleep(1000);

            }
            //wssv.Stop();


        }
    }
}
