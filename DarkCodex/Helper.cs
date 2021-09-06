﻿using DarkCodex.Components;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace DarkCodex
{
    public static class Helper
    {
        #region Other

        public static int MinMax(this int number, int min, int max)
        {
            return Math.Max(min, Math.Min(number, max));
        }

        #endregion

        #region Arrays
        public static T Create<T>(Action<T> action = null) where T : ScriptableObject
        {
            var result = ScriptableObject.CreateInstance<T>();
            if (action != null)
            {
                action(result);
            }
            return result;
        }

        public static T Instantiate<T>(T obj, Action<T> action = null) where T : ScriptableObject
        {
            var result = ScriptableObject.Instantiate<T>(obj);
            if (action != null)
            {
                action(result);
            }
            return result;
        }

        public static T CreateCopy<T>(T original, Action<T> action = null) where T : UnityEngine.Object
        {
            var clone = UnityEngine.Object.Instantiate(original);
            if (action != null)
            {
                action(clone);
            }
            return clone;
        }

        public static T[] ObjToArray<T>(this T obj)
        {
            return new T[] { obj };
        }

        public static T[] ToArray<T>(params T[] objs)
        {
            return objs;
        }

        /// <summary>Appends objects on array.</summary>
        public static T[] Append<T>(T[] orig, params T[] objs)
        {
            if (orig == null) orig = new T[0];

            int i, j;
            T[] result = new T[orig.Length + objs.Length];
            for (i = 0; i < orig.Length; i++)
                result[i] = orig[i];
            for (j = 0; i < result.Length; i++)
                result[i] = objs[j++];
            return result;
        }

        /// <summary>Appends objects on array and overwrites the original.</summary>
        public static T[] AppendAndReplace<T>(ref T[] orig, params T[] objs)
        {
            if (orig == null) orig = new T[0];

            int i, j;
            T[] result = new T[orig.Length + objs.Length];
            for (i = 0; i < orig.Length; i++)
                result[i] = orig[i];
            for (j = 0; i < result.Length; i++)
                result[i] = objs[j++];
            orig = result;
            return result;
        }

        public static T[] AppendAndReplace<T>(ref T[] orig, List<T> objs)
        {
            if (orig == null) orig = new T[0];

            T[] result = new T[orig.Length + objs.Count];
            int i;
            for (i = 0; i < orig.Length; i++)
                result[i] = orig[i];
            foreach (var obj in objs)
                result[i++] = obj;
            orig = result;
            return result;
        }

        #endregion

        #region Log

        /// <summary>Only prints message, if compiled on DEBUG.</summary>
        [System.Diagnostics.Conditional("DEBUG")]
        internal static void PrintDebug(string msg)
        {
            Main.logger?.Log(msg);
        }

        internal static void Print(string msg)
        {
            Main.logger?.Log(msg);
        }

        internal static void PrintException(Exception ex)
        {
            Main.logger?.LogException(ex);
        }

        #endregion

        #region Strings

        public static LocalizedString CreateString(this string value, string key = null) // TODO: get rid of key and make database
        {
            if (key == null)
                key = new Guid().ToString();

            // See if we used the text previously.
            // (It's common for many features to use the same localized text.
            // In that case, we reuse the old entry instead of making a new one.)
            LocalizedString localized;
            //if (_textToLocalizedString.TryGetValue(value, out localized))
            //{
            //    return localized;
            //}
            var strings = LocalizationManager.CurrentPack.Strings;
            //string oldValue;
            //if (strings.TryGetValue(key, out oldValue) && value != oldValue)
//            {
//#if DEBUG
//                Helper.Print($"Info: duplicate localized string `{key}`, different text.");
//#endif
//            }
            strings[key] = value;
            localized = new LocalizedString();
            localized.Key = key;
            //_textToLocalizedString[value] = localized;
            return localized;
        }

        // All localized strings created in this mod, mapped to their localized key. Populated by CreateString.
        public static Dictionary<string, LocalizedString> _textToLocalizedString = new Dictionary<string, LocalizedString>();

        #endregion

        #region Components

        public static void AddComponents(this BlueprintScriptableObject obj, params BlueprintComponent[] components)
        {
            obj.ComponentsArray = Append(obj.ComponentsArray, components);
        }

        public static void SetComponents(this BlueprintScriptableObject obj, IEnumerable<BlueprintComponent> components)
        {
            obj.ComponentsArray = components.ToArray();
        }

        public static void AddFeature(this BlueprintArchetype obj, int level, BlueprintFeatureBase feature)
        {
            var levelentry = obj.AddFeatures.FirstOrDefault(f => f.Level == level);
            if (levelentry != null)
                levelentry.m_Features.Add(feature.ToRef());
            else
                AppendAndReplace(ref obj.AddFeatures, CreateLevelEntry(level, feature));
        }

        public static void RemoveFeature(this BlueprintArchetype obj, int level, BlueprintFeatureBase feature)
        {
            var levelentry = obj.RemoveFeatures.FirstOrDefault(f => f.Level == level);
            if (levelentry != null)
                levelentry.m_Features.Add(feature.ToRef());
            else
                AppendAndReplace(ref obj.RemoveFeatures, CreateLevelEntry(level, feature));
        }

        #endregion

        #region Context

        public static ContextValue CreateContextValue(AbilityRankType value = AbilityRankType.Default)
        {
            return new ContextValue() { ValueType = ContextValueType.Rank, ValueRank = value };
        }

        public static ContextValue CreateContextValue(AbilitySharedValue value)
        {
            return new ContextValue() { ValueType = ContextValueType.Shared, ValueShared = value };
        }

        #endregion

        #region Create Advanced

        public static BlueprintFeatureSelection _basicfeats;
        public static void AddFeats(params BlueprintFeature[] feats)
        {
            if (_basicfeats == null)
                _basicfeats = ResourcesLibrary.TryGetBlueprint<BlueprintFeatureSelection>("247a4068296e8be42890143f451b4b45");
            Helper.AppendAndReplace(ref _basicfeats.m_AllFeatures, feats.ToRef());
        }

        public static ContextCondition[] CreateConditionHasNoBuff(params BlueprintBuff[] buffs)
        {
            if (buffs == null || buffs[0] == null) throw new ArgumentNullException();
            var result = new ContextCondition[buffs.Length];

            for (int i = 0; i < result.Length; i++)
            {
                var buff = new ContextConditionHasBuff();
                buff.m_Buff = buffs[i].ToRef();
                buff.Not = true;
                result[i] = buff;
            }

            return result;
        }

        #endregion

        #region Create

        public static void AddAsset(this SimpleBlueprint bp, string guid) => ResourcesLibrary.BlueprintsCache.AddCachedBlueprint(BlueprintGuid.Parse(guid), bp);
        public static void AddAsset(this SimpleBlueprint bp, Guid guid) => ResourcesLibrary.BlueprintsCache.AddCachedBlueprint(new BlueprintGuid(guid), bp);
        public static void AddAsset(this SimpleBlueprint bp, BlueprintGuid guid) => ResourcesLibrary.BlueprintsCache.AddCachedBlueprint(guid, bp);

        public static AbilityRequirementActionAvailable CreateAbilityRequirementActionAvailable(bool Not, ActionType Action, float Amount = 3f)
        {
            var result = new AbilityRequirementActionAvailable();
            result.Not = Not;
            result.Action = Action;
            result.Amount = Amount;
            return result;
        }

        public static AbilityEffectRunAction CreateAbilityEffectRunAction(SavingThrowType save = SavingThrowType.Unknown, params GameAction[] actions)
        {
            if (actions == null || actions[0] == null) throw new ArgumentNullException();
            var result = new AbilityEffectRunAction();
            result.SavingThrowType = save;
            result.Actions = new ActionList() { Actions = actions };
            return result;
        }

        public static ContextActionRemoveBuff CreateContextActionRemoveBuff(BlueprintBuff buff, bool toCaster = false)
        {
            var result = new ContextActionRemoveBuff();
            result.m_Buff = buff.ToRef();
            result.ToCaster = toCaster;
            return result;
        }

        public static ContextConditionHasBuff CreateContextConditionHasBuff(this BlueprintBuff buff)
        {
            var hasBuff = new ContextConditionHasBuff();
            hasBuff.m_Buff = buff.ToRef();
            return hasBuff;
        }

        public static ActionList CreateActionList(params GameAction[] actions)
        {
            if (actions == null || actions.Length == 1 && actions[0] == null) actions = Array.Empty<GameAction>();
            return new ActionList() { Actions = actions };
        }

        public static Conditional CreateConditional(Condition condition, GameAction ifTrue, GameAction ifFalse = null, bool OperationAnd = true)
        {
            var c = new Conditional();
            c.ConditionsChecker = new ConditionsChecker() { Conditions = condition.ObjToArray(), Operation = OperationAnd ? Operation.And : Operation.Or };
            c.IfTrue = CreateActionList(ifTrue);
            c.IfFalse = CreateActionList(ifFalse);
            return c;
        }

        public static Conditional CreateConditional(Condition[] condition, GameAction[] ifTrue, GameAction[] ifFalse = null, bool OperationAnd = true)
        {
            var c = new Conditional();
            c.ConditionsChecker = new ConditionsChecker() { Conditions = condition, Operation = OperationAnd ? Operation.And : Operation.Or };
            c.IfTrue = CreateActionList(ifTrue);
            c.IfFalse = CreateActionList(ifFalse);
            return c;
        }

        public static AbilityRequirementHasBuffs CreateAbilityRequirementHasBuffs(bool Not, params BlueprintBuff[] Buffs)
        {
            var result = new AbilityRequirementHasBuffs();
            result.Not = Not;
            result.Buffs = Buffs;
            return result;
        }

        public static AbilityRequirementHasBuffTimed CreateAbilityRequirementHasBuffTimed(CompareType Compare, TimeSpan TimeLeft, params BlueprintBuff[] Buffs)
        {
            var result = new AbilityRequirementHasBuffTimed();
            result.Compare = Compare;
            result.Buffs = Buffs;
            result.TimeLeft = TimeLeft;
            return result;
        }

        public static ContextActionApplyBuff CreateContextActionApplyBuff(BlueprintBuff buff, int duration = 0, DurationRate rate = DurationRate.Rounds, bool dispellable = false, bool permanent = false)
        {
            return CreateContextActionApplyBuff(buff, CreateContextDurationValue(bonus: new ContextValue() { Value = duration }, rate: rate), fromSpell: false, dispellable: dispellable, permanent: permanent);
        }

        public static ContextDurationValue CreateContextDurationValue(ContextValue bonus = null, DurationRate rate = DurationRate.Rounds, DiceType diceType = DiceType.Zero, ContextValue diceCount = null)
        {
            return new ContextDurationValue()
            {
                BonusValue = bonus ?? CreateContextValue(),
                Rate = rate,
                DiceCountValue = diceCount ?? 0,
                DiceType = diceType
            };
        }

        public static ContextActionApplyBuff CreateContextActionApplyBuff(this BlueprintBuff buff, ContextDurationValue duration, bool fromSpell, bool dispellable = true, bool toCaster = false, bool asChild = false, bool permanent = false)
        {
            var result = new ContextActionApplyBuff();
            result.m_Buff = buff.ToRef();
            result.DurationValue = duration;
            result.IsFromSpell = fromSpell;
            result.IsNotDispelable = !dispellable;
            result.ToCaster = toCaster;
            result.AsChild = asChild;
            result.Permanent = permanent;
            return result;
        }

        public static BlueprintBuff CreateBlueprintBuff(string name, string displayName, string description, string guid = null, Sprite icon = null, PrefabLink fxOnStart = null, params BlueprintComponent[] components)
        {
            if (guid == null)
                guid = GuidManager.i.Get(name);

            var result = new BlueprintBuff();
            result.name = name;
            result.m_DisplayName = displayName.CreateString();
            result.m_Description = description.CreateString();
            result.AssetGuid = BlueprintGuid.Parse(guid);
            result.m_Icon = icon;
            result.ComponentsArray = components;
            result.FxOnStart = fxOnStart ?? new PrefabLink();
            result.FxOnRemove = new PrefabLink();
            result.IsClassFeature = true;

            AddAsset(result, result.AssetGuid);
            return result;
        }

        public static LevelEntry CreateLevelEntry(int level, params BlueprintFeatureBase[] features) // TODO: complete
        {
            var result = new LevelEntry();
            result.Level = level;
            result.m_Features = features.Select(s => s.ToRef()).ToList();
            return result;
        }

        public static ClassLevelsForPrerequisites CreateClassLevelsForPrerequisites(string target_class, int bonus = 0, string source_class = null, double multiplier = 1d)
        {
            var result = new ClassLevelsForPrerequisites();
            result.m_FakeClass = new BlueprintCharacterClassReference();
            result.m_FakeClass.ReadGuidFromJson(target_class);
            result.m_ActualClass = new BlueprintCharacterClassReference();
            result.m_ActualClass.ReadGuidFromJson(source_class);
            result.Summand = bonus;
            result.Modifier = multiplier;
            return result;
        }

        public static PrerequisiteClassLevel CreatePrerequisiteClassLevel(BlueprintCharacterClass @class, int level, bool any = false)
        {
            var result = new PrerequisiteClassLevel();
            result.m_CharacterClass = @class.ToRef();
            result.Level = level;
            result.Group = any ? Prerequisite.GroupType.Any : Prerequisite.GroupType.All;
            return result;
        }

        public static AddFacts CreateAddFacts(params BlueprintUnitFact[] facts)
        {
            var result = new AddFacts();
            result.m_Facts = facts.Select(s => s.ToRef()).ToArray();
            return result;
        }

        public static BlueprintFeature CreateBlueprintFeature(string name, string displayName, string description, string guid = null, Sprite icon = null, FeatureGroup group = 0, params BlueprintComponent[] components)
        {
            if (guid == null)
                guid = GuidManager.i.Get(name);

            var result = new BlueprintFeature();
            result.name = name;
            result.m_DisplayName = displayName.CreateString();
            result.m_Description = description.CreateString();
            result.AssetGuid = BlueprintGuid.Parse(guid);
            result.m_Icon = icon;
            result.Groups = group == 0 ? Array.Empty<FeatureGroup>() : ToArray(group);
            result.ComponentsArray = components;

            AddAsset(result, result.AssetGuid);
            return result;
        }

        public static BlueprintAbility CreateBlueprintAbility(string name, string displayName, string description, string guid, Sprite icon, AbilityType type, CommandType actionType, AbilityRange range, string duration, string savingThrow, params BlueprintComponent[] components)
        {
            if (guid == null)
                guid = GuidManager.i.Get(name);

            var result = new BlueprintAbility();
            result.name = name;
            result.m_DisplayName = displayName.CreateString();
            result.m_Description = description.CreateString();
            result.AssetGuid = BlueprintGuid.Parse(guid);
            result.m_Icon = icon;
            result.ComponentsArray = components;
            result.ResourceAssetIds = Array.Empty<string>();
            result.Type = type;
            result.ActionType = actionType;
            result.Range = range;
            result.LocalizedDuration = CreateString(duration);
            result.LocalizedSavingThrow = CreateString(savingThrow);

            AddAsset(result, result.AssetGuid);
            return result;
        }

        #endregion

        #region ToReference

        public static BlueprintFeatureReference ToRef(this BlueprintFeature feature)
        {
            //feature.ToReference<BlueprintFeatureReference>();
            var result = new BlueprintFeatureReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }
        public static BlueprintFeatureReference[] ToRef(this BlueprintFeature[] feature)
        {
            var result = new BlueprintFeatureReference[feature.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new BlueprintFeatureReference();
                result[i].deserializedGuid = feature[i].AssetGuid;
            }
            return result;
        }

        public static BlueprintCharacterClassReference ToRef(this BlueprintCharacterClass feature)
        {
            var result = new BlueprintCharacterClassReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }

        public static BlueprintFeatureBaseReference ToRef(this BlueprintFeatureBase feature)
        {
            var result = new BlueprintFeatureBaseReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }

        public static BlueprintBuffReference ToRef(this BlueprintBuff feature)
        {
            var result = new BlueprintBuffReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }

        public static BlueprintUnitFactReference ToRef(this BlueprintUnitFact feature)
        {
            var result = new BlueprintUnitFactReference();
            result.deserializedGuid = feature.AssetGuid;
            return result;
        }

        #endregion

        #region Image

        public static Sprite CreateSprite(string filename, int width = 64, int height = 64)
        {
            try
            {
                var bytes = File.ReadAllBytes(Path.Combine(Main.ModPath, "Icons", filename));
                var texture = new Texture2D(width, height);
                texture.LoadImage(bytes);
                return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0, 0));
            }
            catch (Exception e)
            {
                PrintException(e);
                return null;
            }
        }

        #endregion
    }
}
