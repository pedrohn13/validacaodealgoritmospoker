using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace TesteHandPoker
{
    public class Program
    {
        public static List<Card> Deck;
        public static Player[] Players;

        public static List<Card> CommonCards { get; private set; }

        public static void Main(string[] args)
        {
            //Deck = CreateDeck();


            Player p1 = new Player("A", 0) { Cards = new Card[] { new Card(4, Suit.CLUB), new Card(9, Suit.SPADE) }, BestHand = new Hand() { cards = new Card[] { new Card(10, Suit.DIAMOND) }.ToList(), handType = HandType.HIGH_CARD } };
            Player p2 = new Player("B", 0) { Cards = new Card[] { new Card(5, Suit.DIAMOND), new Card(3, Suit.DIAMOND) }, BestHand = new Hand() { cards = new Card[] { new Card(10, Suit.DIAMOND) }.ToList(), handType = HandType.HIGH_CARD } };
            Player p3 = new Player("C", 0) { Cards = new Card[] { new Card(4, Suit.HEART), new Card(2, Suit.HEART) }, BestHand = new Hand() { cards = new Card[] { new Card(10, Suit.DIAMOND),  }.ToList(), handType = HandType.HIGH_CARD } };
            Players = new Player[] { p1, p2, p3 };
            CommonCards = new List<Card>() { new Card(10, Suit.DIAMOND), new Card(4, Suit.CLUB), new Card(6, Suit.SPADE), new Card(7, Suit.SPADE), new Card(2, Suit.DIAMOND) };


            //Hand auxHand = HandVerification.Straight(pc, cc);
            //Console.WriteLine(auxHand.handType);
            //foreach (Card card in auxHand.cards)
            //{
            //    Console.WriteLine(card.ToString());
            //}


            // Validate();
            //CompareHands();
            //var x = ResolveDraw(new List<int>() {  });
            //foreach (var a in x)
            //{
            //    Console.WriteLine(Players[a].Name);
            //}
            //Console.ReadKey();
        }

        private static List<int> ResolveDraw(List<int> bestHandsPos)
        {
            if (bestHandsPos.Count <= 1)
            {
                return bestHandsPos;
            }
            else
            {
                int amountOfKicker = 0;
                int sizeOfRemain = 0;
                Dictionary<int, List<Card>> dic = new Dictionary<int, List<Card>>();
                switch (Players[bestHandsPos[0]].BestHand.handType)
                {
                    case HandType.HIGH_CARD:
                        amountOfKicker = 4;
                        break;
                    case HandType.ONE_PAIR:
                        amountOfKicker = 3;
                        break;
                    case HandType.THREE_OF_A_KIND:
                        amountOfKicker = 2;
                        break;
                    case HandType.TWO_PAIR:
                    case HandType.FOUR_OF_A_KIND:
                        amountOfKicker = 1;
                        break;
                    case HandType.STRAIGHT:
                    case HandType.FLUSH:
                    case HandType.FULL_HOUSE:
                    case HandType.STRAIGHT_FLUSH:
                        return bestHandsPos;
                }

                /* Cria a lista de cartas sobrantes de cada jogador. De forma que ele faz uma união das
                 cartas comunitarias com as da mao, após isso, remove as cartas da mao vitoriosa para sobrar
                 só as que serão usadas no desempate, baseada na quantidade necessária para fazer o desempate.
                 */
                foreach (int index in bestHandsPos)
                {
                    //Lista ordenada das cartas remanecentes
                    List<Card> remainCards = GetAuxListWithoutBestCards(Players[index], amountOfKicker);
                    dic.Add(index, remainCards);
                    sizeOfRemain = remainCards.Count;
                }

                List<int> result = new List<int>();
                List<int> eliminated = new List<int>();
                //varre a quantidade de cartas sobrantes, no caso do par, sobram 3
                for (int i = 0; i < sizeOfRemain; i++)
                {
                    int highest = -1;
                    //varre cada jogador
                    foreach (var key in dic.Keys)
                    {
                        //Verifica se o jogador nao esta na lista de eliminados
                        if (!eliminated.Contains(key))
                        {
                            //se a carta i do jogador na lista de sobrantes for um ás, ele entra, pois ás eh maior de todas
                            if (dic[key][i].Value == 1)
                            {
                                highest = 99;
                                result.Clear();
                                result.Add(key);
                            }
                            //se a carta i do jogador na lista de sobrantes for maior q a maior atual
                            else if (dic[key][i].Value > highest)
                            {
                                //atualiza a maior atual, limpa a lista com os maiores, e adiciona na lista dos maiores a key do jogador
                                highest = dic[key][i].Value;
                                result.Clear();
                                result.Add(key);
                            }
                            else if (dic[key][i].Value == highest)
                            {
                                //se for igual adiciona ele na lista dos maiores
                                result.Add(key);
                            }
                            else
                            {
                                eliminated.Add(key);
                            }
                        }
                    }

                    //Se o resultado só tem um jogador, eh pq ja achou o vencedor
                    if (result.Count == 1)
                    {
                        break;
                    }
                    // verifica se ainda tem mais de um jogador para comparar cartas e se não é a ultima carta que foi comparada
                    else if (i < sizeOfRemain - 1 &&
                             result.Count == dic.Keys.Count - eliminated.Count)
                    {
                        // se sim, ele limpa a lista para fazer a comparacao com a proxima carta
                        result.Clear();
                    }
                }

                return result;
            }
        }

        public static List<Card> GetAuxListWithoutBestCards(Player player, int amountOfKicker)
        {
            List<Card> aux = new List<Card>();
            List<Card> union = prepareList(player.Cards, CommonCards);
            foreach (Card card in union)
            {
                bool contains = false;
                foreach (Card bestCard in player.BestHand.cards)
                {
                    if (card.Value == bestCard.Value)
                    {
                        contains = true;
                        break;
                    }
                }
                if (!contains)
                {
                    aux.Add(card);
                }
            }
            aux = aux.OrderByDescending(c => c.Value).ToList();
            List<Card> final = new List<Card>();
            for (int i = 0; i < aux.Count; i++)
            {
                if (aux[i].Value == 1)
                {
                    final.Insert(0, aux[i]);
                }
                else
                {
                    final.Add(aux[i]);
                }
            }
            int totalToRemove = final.Count - amountOfKicker;
            final.RemoveRange(final.Count - totalToRemove, totalToRemove);
            return final;
        }

        public static List<Card> prepareList(Card[] playerCards, List<Card> boardCards)
        {
            List<Card> lPlayerCards = playerCards.OfType<Card>().ToList();
            return lPlayerCards.Union(boardCards).OrderBy(c => c.Suit).ThenBy(c => c.Value).ToList();
        }

        private static void CompareHands()
        {

            ShuffleCards();
            Random rnd = new Random();
            int totalP = rnd.Next(2, 6);

            for (int i = 0; i < totalP; i++)
            {
                int pos = rnd.Next(5);
                while (Players[pos] != null)
                {
                    pos = rnd.Next(5);
                }
                Player p = new Player("player" + pos, 1000);
                int firstCardInd = i * 2;
                p.Cards = new Card[] { Deck[firstCardInd], Deck[firstCardInd + 1] };
                p.SitDown(pos);
                Players[pos] = p;
            }

            int cardInd = totalP * 2;
            List<Card> cc = new List<Card>() { Deck[cardInd], Deck[cardInd + 1], Deck[cardInd + 2], Deck[cardInd + 3], Deck[cardInd + 4] };

            foreach (Player p in Players)
            {
                if (p != null)
                {
                    p.GetHighestHand(cc);
                }

            }

            int best = GetBestHandPlayerPosition();
            Console.ReadKey();
        }

        private static void Validate()
        {
            int count = 0;

            while (true)
            {
                ShuffleCards();
                count++;
                Card[] pc = new Card[] { Deck[0], Deck[1] };
                List<Card> cc = new List<Card>() { Deck[2], Deck[3], Deck[4] };

                Console.WriteLine("INICIANDO VERIFICAÇÃO");
                Console.WriteLine("MÃO: " + Deck[0].ToString() + ", " + Deck[1].ToString());
                Console.WriteLine("COMMON: " + Deck[2].ToString() + ", " + Deck[3].ToString() + ", " + Deck[4].ToString() + ", " + Deck[5].ToString() + ", " + Deck[6].ToString());
                Hand finalHand = new Hand();
                Hand auxHand = HandVerification.Flush(pc, cc);
                if (auxHand.handType == HandType.FLUSH)
                {
                    Console.WriteLine("TEMOS PELO MENOS UM FLUSH <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
                    finalHand = auxHand;
                    auxHand = HandVerification.StraightFlush(pc, cc);
                    if (auxHand.handType == HandType.STRAIGHT_FLUSH)
                    {
                        Console.WriteLine("TEMOS PELO MENOS UM STRAIGHT_FLUSH <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
                        finalHand = auxHand;
                        auxHand = HandVerification.RoyalFush(pc, cc);
                        if (auxHand.handType == HandType.ROYAL_FLUSH)
                        {
                            Console.WriteLine("TEMOS UM ROYAL_FLUSH <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
                            finalHand = auxHand;

                        }

                    }
                }
                else
                {
                    auxHand = HandVerification.FourOfAKind(pc, cc);
                    if (auxHand.handType == HandType.FOUR_OF_A_KIND)
                    {
                        Console.WriteLine("TEMOS PELO MENOS UM FOUR_OF_A_KIND <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
                        finalHand = auxHand;
                    }
                    else
                    {
                        auxHand = HandVerification.Pair(pc, cc);
                        if (auxHand.handType == HandType.ONE_PAIR)
                        {
                            Console.WriteLine("TEMOS PELO MENOS UM ONE_PAIR <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
                            finalHand = auxHand;

                            auxHand = HandVerification.ThreeOfAKind(pc, cc);
                            if (auxHand.handType == HandType.THREE_OF_A_KIND)
                            {
                                Console.WriteLine("TEMOS PELO MENOS UM THREE_OF_A_KIND <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
                                finalHand = auxHand;

                                auxHand = HandVerification.FullHouse(pc, cc);
                                if (auxHand.handType == HandType.FULL_HOUSE)
                                {
                                    Console.WriteLine("TEMOS UM FULL_HOUSE <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
                                    finalHand = auxHand;
                                }
                            }
                            else
                            {
                                auxHand = HandVerification.TwoPair(pc, cc);
                                if (auxHand.handType == HandType.TWO_PAIR)
                                {
                                    Console.WriteLine("TEMOS UM TWO_PAIR <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
                                    finalHand = auxHand;
                                }
                            }


                        }

                        if (auxHand.handType != HandType.FULL_HOUSE)
                        {
                            auxHand = HandVerification.Straight(pc, cc);
                            if (auxHand.handType == HandType.STRAIGHT)
                            {
                                Console.WriteLine("TEMOS UM STRAIGHT <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
                                finalHand = auxHand;
                            }
                        }

                        if (finalHand.handType == HandType.NONE)
                        {
                            auxHand = HandVerification.HighCard(pc, cc);
                            if (auxHand.handType == HandType.HIGH_CARD)
                            {
                                Console.WriteLine("TEMOS UM HIGH_CARD <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
                                finalHand = auxHand;
                            }
                        }
                    }
                }

                Console.WriteLine(finalHand.cards.Count);
                Console.WriteLine(finalHand.handType);
                foreach (Card card in finalHand.cards)
                {
                    Console.WriteLine(card.ToString());
                }
                Console.WriteLine("TOTAL DE ITERAÇÕES: " + count);
                Console.WriteLine("FIM, PRESSIONE PARA SAIR");
                //Console.ReadKey();

            }
        }

        public static int GetBestHandPlayerPosition()
        {
            Console.WriteLine("=====================================================");
            Console.WriteLine("=====================================================");
            Console.WriteLine("=====================================================");
            int positon = -1;

            for (int i = 0; i < Players.Length; i++)
            {
                if (Players[i] != null)
                {
                    Console.WriteLine(Players[i].BestHand.handType + " MÃO DO JOGADOR DA VEZ");
                    if (positon > -1)
                    {
                        if (Players[i].BestHand.handType > Players[positon].BestHand.handType)
                        {
                            Console.WriteLine(Players[i].BestHand.handType + " maior que " + Players[positon].BestHand.handType);
                            positon = i;
                            Console.WriteLine(Players[positon].BestHand.handType + " É A MÃO MAIOR AGORA <1>");
                        }
                        else if (Players[i].BestHand.handType == Players[positon].BestHand.handType)
                        {
                            Console.WriteLine("EMPATOU MÃO");
                            Console.WriteLine(Players[i].BestHand.handType + " igual a " + Players[positon].BestHand.handType);
                            if (GetHighestValue(Players[i].BestHand.cards) > GetHighestValue(Players[positon].BestHand.cards))
                            {
                                Console.WriteLine(GetHighestValue(Players[i].BestHand.cards) + " maior que " + GetHighestValue(Players[positon].BestHand.cards));
                                positon = i;
                                Console.WriteLine(Players[positon].BestHand.handType + " É A MÃO MAIOR AGORA <2>");
                            }
                            else if (GetHighestValue(Players[i].BestHand.cards) == GetHighestValue(Players[positon].BestHand.cards))
                            {
                                Console.WriteLine("EMPATOU valor");
                                //VERIFICAR CONDIÇÃO DE DESEMPATE NESSE CASO
                                positon = GetPositionsOfHeavyHand(Players[i].BestHand, Players[positon].BestHand);
                                Console.WriteLine(Players[positon].BestHand.handType + " É A MÃO MAIOR AGORA <3>");
                            }
                            else
                            {
                                Console.WriteLine("NÃO EH MAIOR, PASSA");
                            }
                        }
                        else
                        {
                            Console.WriteLine("NÃO EH MELHOR, PASSA");
                        }
                    }
                    else
                    {
                        positon = i;
                        Console.WriteLine(Players[positon].BestHand.handType + " É A MÃO MAIOR AGORA <4>");
                    }
                }
            }

            return positon;
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

        private static int GetPositionsOfHeavyHand(Hand hand1, Hand hand2)
        {
            int highest1 = GetHighestValue(hand1.cards);
            int highest2 = GetHighestValue(hand2.cards);

            if (highest1 > highest2)
            {
                return hand1.playerPosition;
            }
            else if (highest1 < highest2)
            {
                return hand2.playerPosition;
            }

            Hand aux1 = new Hand() { playerPosition = hand1.playerPosition, cards = hand1.cards.Where(c => c.Value != highest1).ToList() };
            Hand aux2 = new Hand() { playerPosition = hand2.playerPosition, cards = hand2.cards.Where(c => c.Value != highest2).ToList() };

            if (aux1.cards.Count == 0)
            {
                // NESSE LOCAL EH ONDE DA O EMPATE 100%, MUDAR PARA RETORNAR LISTA
                return hand1.playerPosition;
            }

            return GetPositionsOfHeavyHand(aux1, aux2);

        }

        private static void ShuffleCards()
        {
            Random rng = new Random();
            int n = Deck.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Card card = Deck[k];
                Deck[k] = Deck[n];
                Deck[n] = card;
            }
        }

        private static List<Card> CreateDeck()
        {
            List<Card> deck = new List<Card>();
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                for (int i = 1; i <= 13; i++)
                    deck.Add(new Card(i, suit));
            }
            return deck;

        }

        private void loko()
        {
            int cont = 0;
            int total = 0;
            for (int a = 0; a < 52; a++)
            {
                for (int b = 0; b < 52; b++)
                {
                    if (b == a)
                    {
                        continue;
                    }
                    for (int c = 0; c < 52; c++)
                    {
                        if (c == a || c == b)
                        {
                            continue;
                        }
                        for (int d = 0; d < 52; d++)
                        {
                            if (d == a || d == b || d == c)
                            {
                                continue;
                            }

                            for (int e = 0; e < 52; e++)
                            {
                                if (e == a || e == b || e == c || e == d)
                                {
                                    continue;
                                }

                                for (int f = 0; f < 52; f++)
                                {
                                    if (f == a || f == b || f == c || f == d || f == e)
                                    {
                                        continue;
                                    }

                                    for (int g = 0; g < 52; g++)
                                    {
                                        if (g == a || g == b || g == c || g == d || g == e || g == f)
                                        {
                                            continue;
                                        }
                                        total++;
                                        //Console.WriteLine(Deck[a].ToString() + ", " + Deck[b].ToString() + ", " + Deck[c].ToString() + ", " + Deck[d].ToString()
                                        //    + ", " + Deck[e].ToString() + ", " + Deck[f].ToString() + ", " + Deck[g].ToString());

                                        Console.WriteLine(674274182400 - total);

                                        Card[] pc = new Card[] { Deck[a], Deck[b] };
                                        List<Card> cc = new List<Card>() { Deck[c], Deck[d], Deck[e], Deck[f], Deck[g] };
                                        Hand auxHand = HandVerification.RoyalFush(pc, cc);
                                        if (auxHand.handType == HandType.ROYAL_FLUSH)
                                        {
                                            cont++;
                                        }
                                    }
                                }


                            }

                        }

                    }
                }
            }
            Console.WriteLine("TOTAL >> " + cont);
            Console.ReadKey();
        }
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
                auxHand = StraightFlush(playerCards, boardCards);
                if (auxHand.handType == HandType.STRAIGHT_FLUSH)
                {
                    finalHand = auxHand;
                    auxHand = RoyalFush(playerCards, boardCards);
                    if (auxHand.handType == HandType.ROYAL_FLUSH)
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
                List<Card> handCards = Pair(playerCards, boardCards).cards;
                if (handCards.Count == 2)
                {
                    playerCards = playerCards.Except(handCards).ToArray();
                    boardCards = boardCards.Except(handCards).ToList();
                    List<Card> handCards2 = ThreeOfAKind(playerCards, boardCards).cards;
                    if (handCards2.Count == 3)
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

            if (all.Count >= 5)
            {
                List<Card> handCards = new List<Card>();

                for (int i = all.Count - 1; i >= 4; i--)
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

        private static List<Card> prepareList(Card[] playerCards, List<Card> boardCards)
        {
            List<Card> lPlayerCards = playerCards.OfType<Card>().ToList();
            return lPlayerCards.Union(boardCards).OrderBy(c => c.Suit).ThenBy(c => c.Value).ToList();
        }

        private static List<Card> GetCardsOfValue(int value, List<Card> cards)
        {
            return cards.Where(card => card.Value == value).ToList();
        }

        private static List<Card> GetCardsOfSuit(Suit suit, List<Card> cards)
        {
            return cards.Where(card => card.Suit == suit).ToList();
        }

        private static Card GetExactCard(int value, Suit suit, List<Card> cards)
        {
            return cards.Where(card => card.Value == value && card.Suit == suit).FirstOrDefault();
        }

    }

    public enum Suit
    {
        CLUB, DIAMOND, HEART, SPADE
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

    public class Hand
    {
        public int playerPosition { get; set; }
        public HandType handType { get; set; }
        public List<Card> cards { get; set; }


        public Hand()
        {
            cards = new List<Card>();
        }
    }

    public enum PlayerState
    {
        WAITING, PLAYING, FOLD
    }

    public class Player
    {
        public String Name { get; set; }
        public int Position { get; set; }
        public Card[] Cards { get; set; }
        public PlayerState PlayerState { get; set; }
        public int Credits { get; set; }
        public int CurrentBet { get; set; }
        public Hand BestHand { get; set; }

        public Player(String name, int credits)
        {
            Name = name;
            Credits = credits;
            Position = -1;
            Cards = new Card[2];
        }

        public void SitDown(int position)
        {
            Position = position;
        }

        public void Standup()
        {
            Position = -1;
        }

        public bool Bet(int value)
        {
            if (Credits - value >= 0)
            {
                CurrentBet = value;
                Credits -= value;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void PayBet()
        {
            Credits -= CurrentBet;
        }

        public void StartToPlay()
        {
            PlayerState = PlayerState.PLAYING;
        }

        public void Fold()
        {
            PlayerState = PlayerState.FOLD;
        }

        public void ReceiveCredits(int credits)
        {
            Credits += credits;
        }

        public Hand GetHighestHand(List<Card> commonCards)
        {
            BestHand = HandVerification.GetBestHand(Cards, commonCards);
            BestHand.playerPosition = Position;
            return BestHand;
        }

        public override string ToString()
        {
            return String.Format("Name: {0}, Position: {1}, PlayerState: {2}, Credits: {3}, CurrentBet: {4}, Cards: {5}",
                Name, Position, PlayerState, Credits, CurrentBet, CardsString());
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
}



