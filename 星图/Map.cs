﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace 星图
{
    /// <summary>
    /// Global Setting for map display. 
    /// </summary>
    /// 

    public enum StarType
    {
        ClassA,
        ClassB,
        ClassF,
        ClassG,
        ClassK,
        ClassM,
        ClassMRG,
        ClassTBD,
        BlackHole,
        Neutron,
        Pulsar,
    }

    public enum LaneType
    {
        HyperLane,
        JumpGate,
        PsychicSpace,
    }

    [DataContract]
    [KnownType(typeof(Star))]
    [KnownType(typeof(Lane))]
    [KnownType(typeof(List<Star>))]
    [KnownType(typeof(List<Lane>))]
    public class Map
    {
        #region 全局随机属性
        private const double _basicStarHabitableChance = 0.05;

        public static List<double> StarTypeChance { get; } = [10, 10, 15, 30, 20, 20, 10, 0.8, 0.4, 0.4];

        public static List<double> StarHabitableChance { get; } =
            [
                0.6 * _basicStarHabitableChance,
                0.6 * _basicStarHabitableChance,
                _basicStarHabitableChance,
                _basicStarHabitableChance,
                _basicStarHabitableChance,
                0.4 * _basicStarHabitableChance,
                0.1 * _basicStarHabitableChance,
                0.4 * _basicStarHabitableChance,
                0,
                0,
                0
            ];
        /// <summary>
        /// Should interpreted as:
        ///     Index: LaneType
        ///     Value: Chance to be that LaneType if generated in random
        /// </summary>
        public static List<double> LaneTypeChance { get; } = [0, 1, 0];
        /// <summary>
        /// Should interpreted as:
        ///     Index: Difficulty Level
        ///     Value: Chance for hyperlane to have that difficulty level if generated in random
        /// </summary>
        public static List<double> LaneDifficultyChance { get; } = [1, 0, 0, 0];
        #endregion

        [DataMember]
        public ObservableCollection<Star> Stars { get; } = [];
        [DataMember]
        public ObservableCollection<Lane> Lanes { get; } = [];

        internal Dictionary<string, Star> StarDict { get; } = [];

        internal void AddStar(Star star)
        {
            Stars.Add(star);
            StarDict[star.Name] = star;
        }

        /// <summary>
        /// If a Star is deleted, then all Lane it has will be deleted, too.
        /// </summary>
        /// <param name="star"></param>
        internal void RemoveStar(Star star)
        {
            Stars.Remove(star);
            StarDict.Remove(star.Name);

            foreach (Lane l in star.Lanes)
            {
                Star neighborStar = l.Endpoint1_Star == star ? l.Endpoint2_Star : l.Endpoint1_Star;
                neighborStar.Lanes.Remove(l);
                Lanes.Remove(l);
            }
        }

        internal void RemoveStar(string starName)
        {
            Star star = StarDict[starName];
            Stars.Remove(star);
            StarDict.Remove(star.Name);

            foreach (Lane l in star.Lanes)
            {
                Star neighborStar = l.Endpoint1_Star == star ? l.Endpoint2_Star : l.Endpoint1_Star;
                neighborStar.Lanes.Remove(l);
                Lanes.Remove(l);
            }
        }

        internal void ConnectStars(Star star1, Star star2)
        {
            Lane lane = new(star1, star2);
            Lanes.Add(lane);
        }

        internal void ConnectStars(string star1_name, string star2_name)
        {
            Star star1 = StarDict[star1_name];
            Star star2 = StarDict[star2_name];

            Lane lane = new(star1, star2);
            Lanes.Add(lane);
        }

        internal void RemoveLane(Lane lane)
        {
            Star? endStar1 = lane.Endpoint1_Star;
            Star? endStar2 = lane.Endpoint2_Star;

            if (endStar1 == null || endStar2 == null)
            {
                throw new Exception("Invalid Operation: Try to remove a uninitialized lane");
            }

            endStar1.Lanes.Remove(lane);
            endStar2.Lanes.Remove(lane);
            Lanes.Remove(lane);
        }

        public void Clean()
        {
            foreach (Star s in Stars)
            {
                s.Visited = false;
            }
        }
    }
}
