
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static ConsoleApp1.Program;

namespace ConsoleApp1
{
    class Program
    {

        static void Main(string[] args)
        {

            //List<Card> all =
            //    new List<Card>() { new Card(10, Suit.DIAMOND), new Card(5, Suit.DIAMOND), new Card(6, Suit.DIAMOND), new Card(7, Suit.HEART), new Card(9, Suit.HEART),
            //        new Card(10, Suit.HEART), new Card(10, Suit.SPADE),
            //        new Card(4, Suit.DIAMOND), new Card(5, Suit.HEART)
            //    };
            ValidateCopairson();

        }

        public static void ValidateCopairson()
        {
            int total = 0;
            int ocurrence = 0;
            while (true)
            {
                List<Card> deck = new List<Card>();
                foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                {
                    for (int i = 1; i <= 13; i++)
                        deck.Add(new Card(i, suit));
                }
                deck = ShuffleCards(deck);
                List<Card> all = new List<Card>() { deck[0], deck[1], deck[2], deck[3], deck[4],
            deck[5],deck[6],
            deck[7],deck[8],};



                List<Card> commonCards = new List<Card>() { all[0], all[1], all[2], all[3], all[4] };
                Card[] playerCards1 = new Card[] { all[5], all[6] };
                Card[] playerCards2 = new Card[] { all[7], all[8] };

                Player p1 = new Player("p1", "", "", 0, "");
                Player p2 = new Player("p1", "", "", 0, "");
                p1.Cards = playerCards1;
                p2.Cards = playerCards2;
                p1.Position = 0;
                p2.Position = 1;
                p1.GetHighestHand(commonCards);
                p2.GetHighestHand(commonCards);

                if (p1.BestHand.handType == HandType.ROYAL_FLUSH && p2.BestHand.handType == HandType.STRAIGHT_FLUSH)
                {
                    List<int> bestHand = GetBestHandPlayerPosition(new Player[] { p1, p2 });
                    if (bestHand[0] == 0)
                    {
                        ocurrence++;
                    } else
                    {
                        Console.WriteLine("FALHOU DEPOIS DE > " + total);
                        Console.ReadKey();
                    }
                }
                total++;
                Console.WriteLine("OK > " + total);


                if (total == 1000000)
                {
                    Console.WriteLine("RODOU 1 MILHAO DE VEZES E NAO FALHOU, TOTAL DE OCORRENCIAS : " + ocurrence);
                    Console.ReadKey();
                    break;
                }
            }
        }

        public static void ValidateHands(List<Card> all)
        {
            List<Card> commonCards = new List<Card>() { all[0], all[1], all[2], all[3], all[4] };
            Card[] playerCards = new Card[] { all[5], all[6] };
            int total = 0;
            while (true)
            {
                total++;
                Hand hand = HandVerification.GetBestHand(playerCards, commonCards);
                if (hand.handType != HandType.HIGH_CARD)
                {
                    Console.WriteLine("FALHOU DEPOIS DE > " + total + " DEU " + hand.handType);
                    Console.WriteLine("COMMON CARDS");
                    foreach (var card in commonCards)
                    {
                        Console.WriteLine(card.ToString());
                    }
                    Console.WriteLine("PLAYER CARDS");
                    foreach (var card in playerCards)
                    {
                        Console.WriteLine(card.ToString());
                    }
                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine("OK > " + total);
                }

                if (total == 1000000)
                {
                    Console.WriteLine("RODOU 1 MILHAO DE VEZES E NAO FALHOU");
                    Console.ReadKey();
                    break;
                }
            }
        }

        public static List<Card> ShuffleCards(List<Card> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Card card = list[k];
                list[k] = list[n];
                list[n] = card;
            }

            return list;
        }

        public static class HandVerification
        {
            public static Hand GetBestHand(Card[] playerCards, List<Card> boardCards)
            {
                Hand finalHand = new Hand();
                Hand auxHand = Flush(playerCards, boardCards);
                if (auxHand.handType == HandType.FLUSH)
                {
                    finalHand = auxHand;
                    auxHand = RoyalFush(playerCards, boardCards);
                    if (auxHand.handType == HandType.ROYAL_FLUSH)
                    {
                        finalHand = auxHand;
                    }
                    else
                    {
                        auxHand = StraightFlush(playerCards, boardCards);
                        if (auxHand.handType == HandType.STRAIGHT_FLUSH)
                        {
                            finalHand = auxHand;
                        }
                    }
                }
                else
                {
                    auxHand = FourOfAKind(playerCards, boardCards);
                    if (auxHand.handType == HandType.FOUR_OF_A_KIND)
                    {
                        finalHand = auxHand;
                    }
                    else
                    {
                        auxHand = Pair(playerCards, boardCards);
                        if (auxHand.handType == HandType.ONE_PAIR)
                        {
                            finalHand = auxHand;

                            auxHand = ThreeOfAKind(playerCards, boardCards);
                            if (auxHand.handType == HandType.THREE_OF_A_KIND)
                            {
                                finalHand = auxHand;

                                auxHand = FullHouse(playerCards, boardCards);
                                if (auxHand.handType == HandType.FULL_HOUSE)
                                {
                                    finalHand = auxHand;
                                }
                            }
                            else
                            {
                                auxHand = TwoPair(playerCards, boardCards);
                                if (auxHand.handType == HandType.TWO_PAIR)
                                {
                                    finalHand = auxHand;
                                }
                            }
                        }

                        if (auxHand.handType != HandType.FULL_HOUSE)
                        {
                            auxHand = Straight(playerCards, boardCards);
                            if (auxHand.handType == HandType.STRAIGHT)
                            {
                                finalHand = auxHand;
                            }
                        }

                        if (finalHand.handType == HandType.NONE)
                        {
                            auxHand = HighCard(playerCards, boardCards);
                            if (auxHand.handType == HandType.HIGH_CARD)
                            {
                                finalHand = auxHand;
                            }
                        }
                    }
                }

                return finalHand;
            }

            public static Hand RoyalFush(Card[] playerCards, List<Card> boardCards)
            {
                Hand hand = new Hand();

                List<Card> all = prepareList(playerCards, boardCards);
                List<Card> aces = GetCardsOfValue(1, all);

                List<Card> handCards = new List<Card>();

                if (aces.Count > 0 && aces.Count < 4)
                {
                    foreach (Card ace in aces)
                    {
                        handCards.Add(ace);
                        for (int value = 13; value > 9; value--)
                        {
                            if (GetExactCard(value, ace.Suit, all) != null)
                            {
                                handCards.Add(new Card(value, ace.Suit));
                            }
                            else
                            {
                                handCards.Clear();
                                break;
                            }
                        }

                        if (handCards.Count == 5)
                        {
                            hand.handType = HandType.ROYAL_FLUSH;
                            hand.cards = handCards.ToList();
                            handCards.Clear();
                            break;
                        }

                    }
                }

                return hand;
            }

            public static Hand StraightFlush(Card[] playerCards, List<Card> boardCards)
            {
                Hand hand = new Hand();

                List<Card> all = prepareList(playerCards, boardCards);

                if (all.Count >= 5)
                {
                    List<Card> handCards = new List<Card>();

                    for (int i = all.Count - 1; i >= 4; i--)
                    {
                        if (all[i].Suit == all[i - 4].Suit && all[i - 4].Value == all[i].Value - 4)
                        {
                            handCards.Add(all[i - 4]);
                            handCards.Add(all[i - 3]);
                            handCards.Add(all[i - 2]);
                            handCards.Add(all[i - 1]);
                            handCards.Add(all[i]);
                        } //verificar quando da a volta


                        if (handCards.Count == 5)
                        {
                            hand.handType = HandType.STRAIGHT_FLUSH;
                            hand.cards = handCards.ToList();
                            break;
                        }
                    }
                }

                return hand;
            }

            public static Hand FourOfAKind(Card[] playerCards, List<Card> boardCards)
            {
                Hand hand = new Hand();


                List<Card> all = prepareList(playerCards, boardCards);

                if (all.Count >= 4)
                {
                    List<Card> handCards = new List<Card>();
                    for (int i = all.Count - 1; i >= 3; i--)
                    {
                        handCards = GetCardsOfValue(all[i].Value, all);

                        if (handCards.Count == 4)
                        {
                            hand.cards = handCards.ToList();
                            hand.handType = HandType.FOUR_OF_A_KIND;
                            break;
                        }

                        handCards.Clear();
                    }
                }

                return hand;
            }

            public static Hand FullHouse(Card[] playerCards, List<Card> boardCards)
            {
                Hand hand = new Hand();


                List<Card> all = prepareList(playerCards, boardCards);

                if (all.Count >= 5)
                {
                    List<Card> handCards = ThreeOfAKind(playerCards, boardCards).cards;
                    if (handCards.Count == 3)
                    {
                        playerCards = playerCards.Except(handCards).ToArray();
                        boardCards = boardCards.Except(handCards).ToList();
                        List<Card> handCards2 = Pair(playerCards, boardCards).cards;
                        if (handCards2.Count == 2)
                        {
                            hand.cards = prepareList(handCards.ToArray(), handCards2);
                            hand.handType = HandType.FULL_HOUSE;
                        }
                    }
                }

                return hand;
            }

            public static Hand Flush(Card[] playerCards, List<Card> boardCards)
            {
                Hand hand = new Hand();

                List<Card> all = prepareList(playerCards, boardCards);

                if (all.Count >= 5)
                {
                    List<Card> handCards = new List<Card>();
                    for (int i = all.Count - 1; i >= 4; i--)
                    {
                        handCards = GetCardsOfSuit(all[i].Suit, all);

                        if (handCards.Count >= 5)
                        {
                            hand.cards = handCards.ToList();
                            hand.handType = HandType.FLUSH;
                            break;
                        }

                        handCards.Clear();
                    }
                }

                return hand;
            }

            public static Hand Straight(Card[] playerCards, List<Card> boardCards)
            {
                Hand hand = new Hand();


                List<Card> all = prepareList(playerCards, boardCards);
                bool hasAce = false;
                Suit suit = new Suit();
                if (all.Any(x => x.Value == 1))
                {
                    hasAce = true;

                    foreach (Card card in all)
                    {
                        if (card.Value == 1)
                        {
                            suit = card.Suit;
                            break;
                        }
                    }

                    all.Add(new Card(14, suit));
                    all.OrderBy(c => c.Suit).ThenBy(c => c.Value).ToList();
                }


                if (all.Count >= 5)
                {
                    List<Card> handCards = new List<Card>();

                    for (int i = all.Count - 1; i >= 0; i--)
                    {
                        handCards.Add(all[i]);
                        for (int value = all[i].Value - 1; value > 0; value--)
                        {
                            Card cardOfValue = GetCardsOfValue(value, all).FirstOrDefault();
                            if (cardOfValue != null)
                            {
                                handCards.Add(cardOfValue);
                            }
                            else
                            {
                                handCards.Clear();
                                break;
                            }

                            if (handCards.Count == 5)
                            {
                                break;
                            }
                        }

                        if (handCards.Count == 5)
                        {
                            hand.cards = handCards.ToList();
                            if (hasAce)
                            {
                                Card cardToRemove = null;
                                foreach (Card card in hand.cards)
                                {
                                    if (card.Value == 14)
                                    {
                                        cardToRemove = card;
                                        break;
                                    }
                                }

                                if (cardToRemove != null)
                                {
                                    hand.cards.Remove(cardToRemove);
                                    hand.cards.Add(new Card(1, cardToRemove.Suit));
                                }
                            }

                            hand.handType = HandType.STRAIGHT;
                            break;
                        }
                        else
                        {
                            handCards.Clear();
                        }
                    }
                }

                return hand;
            }


            public static Hand ThreeOfAKind(Card[] playerCards, List<Card> boardCards)
            {
                Hand hand = new Hand();


                List<Card> all = prepareList(playerCards, boardCards);

                if (all.Count >= 3)
                {
                    List<Card> handCards = new List<Card>();
                    for (int i = all.Count - 1; i >= 2; i--)
                    {
                        handCards = GetCardsOfValue(all[i].Value, all);

                        if (handCards.Count == 3)
                        {
                            if (hand.cards.Count > 0)
                            {
                                if (hand.cards[0].Value < handCards[0].Value || handCards[0].Value == 1)
                                {
                                    hand.cards = handCards.ToList();
                                    hand.handType = HandType.THREE_OF_A_KIND;
                                }
                            }
                            else
                            {
                                hand.cards = handCards.ToList();
                                hand.handType = HandType.THREE_OF_A_KIND;
                            }
                        }

                        handCards.Clear();
                    }
                }

                return hand;
            }

            public static Hand TwoPair(Card[] playerCards, List<Card> boardCards)
            {
                Hand hand = new Hand();


                List<Card> all = prepareList(playerCards, boardCards);

                if (all.Count >= 4)
                {
                    List<Card> handCards = Pair(playerCards, boardCards).cards;
                    if (handCards.Count == 2)
                    {
                        playerCards = playerCards.Except(handCards).ToArray();
                        boardCards = boardCards.Except(handCards).ToList();
                        List<Card> handCards2 = Pair(playerCards, boardCards).cards;
                        if (handCards2.Count == 2)
                        {
                            hand.cards = prepareList(handCards.ToArray(), handCards2);
                            hand.handType = HandType.TWO_PAIR;
                        }
                    }

                }

                return hand;
            }

            public static Hand Pair(Card[] playerCards, List<Card> boardCards)
            {
                Hand hand = new Hand();

                List<Card> all = prepareList(playerCards, boardCards);

                if (all.Count >= 2)
                {
                    List<Card> handCards = new List<Card>();
                    for (int i = all.Count - 1; i >= 1; i--)
                    {
                        handCards = GetCardsOfValue(all[i].Value, all);

                        if (handCards.Count >= 2)
                        {
                            if (hand.cards.Count > 0)
                            {
                                if (hand.cards[0].Value < handCards[0].Value || handCards[0].Value == 1)
                                {
                                    hand.cards = handCards.ToList();
                                    hand.handType = HandType.ONE_PAIR;
                                    if (handCards[0].Value == 1) break;
                                }
                            }
                            else
                            {
                                hand.cards = handCards.ToList();
                                hand.handType = HandType.ONE_PAIR;
                                if (handCards[0].Value == 1) break;
                            }
                        }

                        handCards.Clear();
                    }
                }

                return hand;
            }

            public static Hand HighCard(Card[] playerCards, List<Card> boardCards)
            {
                Hand hand = new Hand();

                List<Card> all = prepareList(playerCards, boardCards).OrderBy(c => c.Value).ToList();

                hand.cards = new List<Card>() { all[0].Value == 1 ? all[0] : all[all.Count - 1] };
                hand.handType = HandType.HIGH_CARD;

                return hand;
            }

            public static List<Card> prepareList(Card[] playerCards, List<Card> boardCards)
            {
                List<Card> lPlayerCards = playerCards.OfType<Card>().ToList();
                return lPlayerCards.Union(boardCards).OrderBy(c => c.Suit).ThenBy(c => c.Value).ToList();
            }

            public static List<Card> GetCardsOfValue(int value, List<Card> cards)
            {
                return cards.Where(card => card.Value == value).ToList();
            }

            public static List<Card> GetCardsOfSuit(Suit suit, List<Card> cards)
            {
                return cards.Where(card => card.Suit == suit).ToList();
            }

            public static Card GetExactCard(int value, Suit suit, List<Card> cards)
            {
                return cards.Where(card => card.Value == value && card.Suit == suit).FirstOrDefault();
            }

        }

        public static List<int> GetBestHandPlayerPosition(Player[] Players)
        {
            List<int> bestHandsPos = new List<int>();

            for (int i = 0; i < Players.Length; i++)
            {
                if (Players[i] != null)
                {
                    if (bestHandsPos.Count > 0)
                    {
                        if (Players[i].BestHand.handType > Players[bestHandsPos[0]].BestHand.handType)
                        {
                            bestHandsPos.Clear();
                            bestHandsPos.Add(i);
                        }
                        else if (Players[i].BestHand.handType == Players[bestHandsPos[0]].BestHand.handType)
                        {
                            int highestNew = GetHighestValue(Players[i].BestHand.cards);
                            int highestOld = GetHighestValue(Players[bestHandsPos[0]].BestHand.cards);

                            if (highestNew == 1 && highestOld != 1)
                            {
                                bestHandsPos.Clear();
                                bestHandsPos.Add(i);
                            }
                            else if (highestNew != 1 && highestOld == 1)
                            {
                                continue;
                            }
                            else if (highestNew == 1 && highestOld == 1)
                            {
                                bestHandsPos.Add(i);
                            }
                            else if (highestNew > highestOld)
                            {
                                bestHandsPos.Clear();
                                bestHandsPos.Add(i);
                            }
                            else if (highestNew == highestOld)
                            {
                                List<int> positons = GetPositionsOfHeavyHand(Players[i].BestHand, Players[bestHandsPos[0]].BestHand);
                                if (positons.Count == 2)
                                {
                                    bestHandsPos.Add(i);
                                }
                                else
                                {
                                    if (positons[0] == i)
                                    {
                                        bestHandsPos.Clear();
                                        bestHandsPos.Add(i);
                                    }
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        bestHandsPos.Add(i);
                    }
                }
            }

            return bestHandsPos;
        }

        private static List<int> GetPositionsOfHeavyHand(Hand hand1, Hand hand2)
        {
            int highest1 = GetHighestValue(hand1.cards);
            int highest2 = GetHighestValue(hand2.cards);

            if (highest1 > highest2)
            {
                return new List<int>() { hand1.playerPosition };
            }
            else if (highest1 < highest2)
            {
                return new List<int>() { hand2.playerPosition };
            }

            Hand aux1 = new Hand() { playerPosition = hand1.playerPosition, cards = hand1.cards.Where(c => c.Value != highest1).ToList() };
            Hand aux2 = new Hand() { playerPosition = hand2.playerPosition, cards = hand2.cards.Where(c => c.Value != highest2).ToList() };

            if (aux1.cards.Count == 0)
            {
                return new List<int>() { hand1.playerPosition, hand2.playerPosition };
            }

            return GetPositionsOfHeavyHand(aux1, aux2);

        }

        private static int GetHighestValue(List<Card> cards)
        {
            int max = 0;
            foreach (Card card in cards)
            {
                if (card.Value == 1)
                {
                    return 1;
                }

                if (card.Value > max)
                {
                    max = card.Value;
                }
            }

            return max;
        }

    }

    public class Hand
    {
        public int playerPosition { get; set; }
        public HandType handType { get; set; }
        public List<Card> cards { get; set; }


        public Hand()
        {
            cards = new List<Card>();
        }

        public override string ToString()
        {
            return String.Format("HandType: " + handType.ToString() + " - Cards: " + cards.ToString());
        }
    }

    public enum HandType
    {
        NONE, HIGH_CARD, ONE_PAIR, TWO_PAIR, THREE_OF_A_KIND, STRAIGHT, FLUSH, FULL_HOUSE, FOUR_OF_A_KIND, STRAIGHT_FLUSH, ROYAL_FLUSH
    }

    public class Card
    {
        public int Value { get; set; }
        public Suit Suit { get; set; }

        public Card(int value, Suit suit)
        {
            Value = value;
            Suit = suit;
        }

        public override string ToString()
        {
            return String.Format("{0}/{1}", Suit, Value);
        }
    }

    public enum Suit
    {
        CLUB, DIAMOND, HEART, SPADE
    }

    public class Player
    {
        public String Name { get; set; }
        public String Email { get; set; }
        public String Token { get; set; }
        public String PlayerPicture { get; set; }
        public int Position { get; set; }
        public Card[] Cards { get; set; }
        public PlayerState PlayerState { get; set; }
        public int Credits { get; set; }
        public int CurrentBet { get; set; }
        public Hand BestHand { get; set; }
        public bool AllIn { get; set; }

        public Player(String name, String token, String email, int credts, String playerPicture)
        {
            Name = name;
            Token = token;
            Email = email;
            Credits = credts;
            Position = -1;
            Cards = new Card[2];
            PlayerPicture = playerPicture;
        }

        public void SitDown(int position)
        {
            Position = position;
        }

        public Hand GetHighestHand(List<Card> commonCards)
        {
            BestHand = HandVerification.GetBestHand(Cards, commonCards);
            BestHand.playerPosition = Position;
            return BestHand;
        }

        public override string ToString()
        {
            return String.Format("Name: {0}, Position: {1}, PlayerState: {2}, Credits: {3}, CurrentBet: {4}, BetAll: {5}, BestHand: {6}, Cards: {7}",
                Name, Position, PlayerState, Credits, CurrentBet, AllIn, BestHand?.ToString(), CardsString());
        }

        private string CardsString()
        {
            string status = "";
            foreach (Card card in Cards)
            {
                if (card != null)
                {
                    status += String.Format("/[{0}]/", card.ToString());
                }
                else
                {
                    status += "/[]/";
                }
            }
            return status;
        }

    }

    public enum PlayerState
    {
        WAITING, PLAYING, FOLD, OFF
    }
}

