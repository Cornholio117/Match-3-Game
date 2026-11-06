using System.Collections.Generic;
using Game.Core.Flow;
using NUnit.Framework;

namespace Game.Tests.EditMode.Flow
{
    public sealed class StateMachineTests
    {
        // A stand-in state that records every Enter and Exit, so the order can be checked.
        private sealed class LoggingState : IState
        {
            private readonly string _name;
            private readonly List<string> _log;

            public LoggingState(string name, List<string> log)
            {
                _name = name;
                _log = log;
            }

            public void Enter()
            {
                _log.Add(_name + ".Enter");
            }

            public void Exit()
            {
                _log.Add(_name + ".Exit");
            }
        }

        [Test]
        public void ChangeState_FirstState_OnlyEnters()
        {
            var log = new List<string>();
            var machine = new StateMachine();

            machine.ChangeState(new LoggingState("Boot", log));

            Assert.That(log, Is.EqualTo(new[] { "Boot.Enter" }));
        }

        [Test]
        public void ChangeState_SecondState_ExitsOldThenEntersNew()
        {
            var log = new List<string>();
            var machine = new StateMachine();
            machine.ChangeState(new LoggingState("Boot", log));

            machine.ChangeState(new LoggingState("Menu", log));

            Assert.That(log, Is.EqualTo(new[] { "Boot.Enter", "Boot.Exit", "Menu.Enter" }));
        }

        [Test]
        public void ChangeState_SameState_DoesNothing()
        {
            var log = new List<string>();
            var machine = new StateMachine();
            var menu = new LoggingState("Menu", log);
            machine.ChangeState(menu);

            machine.ChangeState(menu);

            Assert.That(log, Is.EqualTo(new[] { "Menu.Enter" }));
        }

        [Test]
        public void ChangeState_FullGameFlow_RunsInOrder()
        {
            var log = new List<string>();
            var machine = new StateMachine();
            var boot = new LoggingState("Boot", log);
            var menu = new LoggingState("Menu", log);
            var level = new LoggingState("Level", log);
            var win = new LoggingState("Win", log);

            machine.ChangeState(boot);
            machine.ChangeState(menu);
            machine.ChangeState(level);
            machine.ChangeState(win);

            Assert.That(log, Is.EqualTo(new[]
            {
                "Boot.Enter",
                "Boot.Exit", "Menu.Enter",
                "Menu.Exit", "Level.Enter",
                "Level.Exit", "Win.Enter"
            }));
            Assert.That(machine.Current, Is.SameAs(win));
        }
    }
}
