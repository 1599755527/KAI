using System;
using KAI.FSA; // Use the namespace from your FSAImpl definition

namespace Miner49er
{
    /// <summary>
    /// This class implements the Miner as described by the first state transition table
    /// </summary>
    public class SimpleMiner : FSAImpl, Miner
    {
        /// Amount of gold nuggest in the miner's pockets ...
        public int gold = 0;
        /// How thirsty the miner is ...
        public int thirst = 0;
        /// How many gold nuggets the miner has in the bank ...
        public int bank = 0;

        // The following variables are each oen of the defiend states the miner cna be in.
        State miningState;
        State drinkingState;
        State bankingState;

        // FIXED: Added : base("SimpleMiner") to resolve the FSAImpl constructor error
        public SimpleMiner() : base("SimpleMiner")
        {
            // FIXED: Using PascalCase to match your FSAImpl.MakeNewState method
            miningState = MakeNewState("Mining");
            drinkingState = MakeNewState("Drinking");
            bankingState = MakeNewState("Banking");

            // set mining transitions
            miningState.addTransition("tick",
                new ConditionDelegate[] { new ConditionDelegate(this.parched) },
                new ActionDelegate[] { new ActionDelegate(this.incrementThirst) }, drinkingState);
            
            miningState.addTransition("tick",
                new ConditionDelegate[] { new ConditionDelegate(this.pocketsFull) },
                new ActionDelegate[] { new ActionDelegate(this.incrementThirst) }, bankingState);
            
            miningState.addTransition("tick",
                new ConditionDelegate[] { }, 
                new ActionDelegate[] { new ActionDelegate(this.dig) }, miningState);

            // set drinking transitions
            drinkingState.addTransition("tick",
                new ConditionDelegate[] { new ConditionDelegate(this.thirsty) },
                new ActionDelegate[] { new ActionDelegate(this.takeDrink) }, drinkingState);
            
            drinkingState.addTransition("tick",
                new ConditionDelegate[] { },
                new ActionDelegate[] { new ActionDelegate(this.incrementThirst) }, miningState);

            // set banking transitions
            // --- Banking Transitions 优化后的顺序 ---

            // 1. 优先级最高：如果身上还有钱，先执行“一键存款” (Action 已修正)
            bankingState.addTransition("tick",
                new ConditionDelegate[] { new ConditionDelegate(this.pocketsNotEmpty) },
                new ActionDelegate[] { new ActionDelegate(this.depositGold) }, bankingState);

            // 2. 优先级次之：存完钱了（口袋空了），检查是否需要“顺路去酒吧” (优化效率)
            bankingState.addTransition("tick",
                new ConditionDelegate[] { (fsa) => thirst > 10 },
                new ActionDelegate[] { }, drinkingState);

            // 3. 最后优先级：如果不满足上述条件，则回矿场并“消除空白帧”直接开挖 (提升效率)
            bankingState.addTransition("tick",
                new ConditionDelegate[] { },
                new ActionDelegate[] { new ActionDelegate(this.dig) }, miningState);

            // FIXED: Using PascalCase to match your FSAImpl.SetCurrentState method
            SetCurrentState(miningState);
        }

        /// <summary>
        /// This is a condition that tests to see if the miner is so thirsty that he cannot dig
        /// </summary>
        private Boolean parched(FSA fsa)
        {
            if (thirst >= 15)
            {
                Console.WriteLine("Too thirsty too work.");
            }
            return thirst >= 15;
        }

        /// <summary>
        /// An action that decrements the miner's thirst ...
        /// </summary>
        private void takeDrink(FSA fsa)
        {
            thirst = 0; 
            Console.WriteLine("Such a generous drinker！");
        }

        /// <summary>
        /// An action that decrements the gold in the miner's pockets and increments the gold in the bank ...
        /// </summary>
        private void depositGold(FSA fsa)
        {
            bank += gold;
            gold = 0;
            Console.WriteLine("deposit all gold");
        }

        /// <summary>
        /// This implements the Miner.getCurrentWealth() call ...
        /// </summary>
        public int getCurrentWealth()
        {
            return bank + gold;
        }

        // --- Previously extracted methods ---

        private void dig(FSA fsa)
        {
            gold++;
            thirst++;
            Console.WriteLine("Miner is digging.");
        }

        private void incrementThirst(FSA fsa)
        {
            thirst++;
        }

        private Boolean pocketsFull(FSA fsa) => gold >= 5;

        private Boolean pocketsNotEmpty(FSA fsa) => gold > 0;

        private Boolean thirsty(FSA fsa) => thirst > 0;

        public void printStatus()
        {
            Console.WriteLine("Thirst: "+thirst+" Gold: "+gold+" Bank: "+bank);
        }
    }
}