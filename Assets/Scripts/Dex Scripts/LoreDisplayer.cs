using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LoreDisplayer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI loreText;

    private Dictionary<string, string> loreEntries;

    void Start() 
    {
        InitializeLoreEntries();
    }

    private void InitializeLoreEntries()
    {
        loreEntries = new Dictionary<string, string>
        {
            { "The Gambler (Pistol Person)", "The weakest soldier in terms of raw power, effective only against weaker robots. He uses his questionable pistol to try and defeat clankers, he went into the casino to gamble his money, but instead he gambled his life against the clanker army to protect his easily earned money" },
            { "Rifle Dude", "Slightly stronger than The Gambler, Rifle Dude serves as an early-to-mid-tier unit. While still weak against hordes and tougher enemies, he provides more consistent damage. His mysterious nature—appearing without explanation—adds intrigue, though his contribution to the team is unquestioned." },
            { "AR Fanboy", "One of the strongest soldiers available and highly proficient with weapons. Armed with an assault rifle, he is capable of handling most enemy types effectively. Lore wise AR Fanboy actually has a massive collection of AR’s at home. They’re in every room of his house, and are even in his bed. AR fanboy actually celebrated the robot uprising as it gave him the excuse to finally use his favorite things in the world, AR’s." },
            { "Laaser Daazer", "The strongest soldier in the game. Equipped with a powerful laser cannon, he can eliminate virtually any robot with ease. Lore Wise, near the end of the Cold War the Soviet Union never collapsed, and decades later it fell in the battle of Moscow to the Clanker army. The remaining soldiers of the Soviet army fled to many countries using their unique weapons like the LaZer Chunker to destroy all who opposed them, many joined the Republic of the American States to fight the Clankers." },
            { "Mafia Boss", "A high-damage but slow-firing soldier who can attack across up to three lanes. While relatively weak defensively, his extreme damage output makes him valuable for controlling key areas. After the near collapse of the Mafia empires in the German Automaton cleansing, few mafia empires survived the cleansing but the many that were destroyed left leaders, and war lords behind leaving the Mafia Boss alone, and vengeful against the Clankers. The remaining Mafia empires and remnants have the goal of destroying all Clankers, by any means necessary." },
            { "Solaridon Soldier", "A high damage soldier from the future, it can take hits from the most powerful enemies, break enemy lines singlehandedly, and turn the tables against the clankers. This soldier deals high damage, has lots of health, and shoots very fast, but be careful, as they are very rare to come by." },
            { "Crossbow Hunter", "A high damage soldier, but shoots slow, its arrows glow in a green hue and can pierce through any enemy in a lane, dealing high damage to numerous clankers in 1 shot. The crossbow hunter deals an increased damage to flying enemy types and excels at clearing them quickly." },
            { "Riot Shield Soldier", "After his squad got eliminated by the 14th clanker regiment, he stands alone to face the hordes of clankers, but his trusty riot shield protects him from insane amounts of attacks, he can't shoot, but can protect his fellow soldiers behind him and achieve his dream goal of protecting his friends." },
            { "Water Bomb & EMP Grenade", "The 2 most useful explosives against the clanker horde, these explosives can either stun enemies or deal large amounts of damage, ideal for assisting soldiers in their endeavour to Rip and Tear. The origins of the Water Bomb stem from the 3rd Clanker attack in Paris, France, when the Clankers attempted to steal a picture of someone stealing the moon and the first clanker resistance designed an explosive that is highly effective against the Clanker horde unlike other explosives. The EMP grenade designed by Black Umbrella research specifically intended to paralyze Clankers proved useful in the 15th Clanker attack of Vancouver, the Clankers were stalled by 6 days with the use of the EMP grenade before the city was lost." },
            { "Roomba", "The most common enemy. Low health, low damage, and moderate speed. Individually weak but dangerous in large swarms. They have become the symbolic face of the Clanker army after being hacked during the uprising." },
            { "Toaster", " Similar to Roombas but slightly slower. Not threatening alone, but effective in groups. Its existence confounded scientists due to its lack of modern tracking or computing hardware, making its autonomy mysterious." },
            { "Smart Fridge", "A major threat with extremely high health and solid damage output. Very slow, giving players time to prepare defenses. Its lore stems from a massive corporate security breach that granted smart appliances sentience." },
            { "Smart Lawnmower", "Fast-moving and fragile but capable of instantly killing most soldiers. On its own it is manageable, but when paired with other enemies it can quickly dismantle defenses. Originates from a security flaw in consumer tech software." },
            { "Smart Washing Machine", "AA ranged enemy that fires water beams to clear defenses. It has high health but extremely slow movement, making it dangerous if ignored. Its lore satirizes aggressive corporate control and warranty enforcement." },
            { "The Drone", "A flying enemy with low health and damage, but moderate speed. It cannot be attacked by most soldiers and can only be countered by Rifle Dude, making it a targeted threat that tests unit composition. Its lore emphasizes loss of ownership and autonomy in modern technology." },
         
            // Add more lore entries as needed
        };
    }

    // Call this method from your buttons
    public void DisplayLore(string loreKey)
    {
        if (loreEntries.ContainsKey(loreKey))
        {
            loreText.text = loreEntries[loreKey];
        }
        else
        {
            Debug.LogWarning($"Lore key '{loreKey}' not found in loreEntries dictionary.");
        }
    }

    // Optional: Method to add new lore entries dynamically
    public void AddLoreEntry(string key, string text)
    {
        loreEntries[key] = text;
    }

    // Optional: Method to clear the text
    public void ClearLore()
    {
        loreText.text = "";
    }
}
