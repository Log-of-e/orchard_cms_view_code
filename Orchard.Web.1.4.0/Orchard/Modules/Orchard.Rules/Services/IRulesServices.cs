﻿using Orchard.Rules.Models;

namespace Orchard.Rules.Services {
    public interface IRulesServices : IDependency {
        RuleRecord CreateRule(string name);
        RuleRecord GetRule(int id);

        void DeleteEvent(int eventId);
        void DeleteAction(int actionId);

        void MoveUp(int actionId);
        void MoveDown(int actionId);
    }
}