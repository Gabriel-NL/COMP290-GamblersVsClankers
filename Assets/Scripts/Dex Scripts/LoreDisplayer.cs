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
            { "Rifle Dude", "Slightly stronger than The Gambler, Rifle Dude serves as an early-to-mid-tier unit. While still weak against hordes and tougher enemies, he provides more consistent damage. Rifle Dude was unfortunately a part of the first few million casualties in the unprising. Rifle dude had to first watch his home be destroyed, Next he had to watch his family be killed one by one by the Clankers until he was all that was left. Lost in his grief and depression it wasn’t long before the Clankers got to Rifle dude next. However due to the help of a certain witch, Rifle Dude came back alive as an undead soldier. Now fueled with an undying vengence towards the Clankers that tore his whole world apart." },
            { "AR Fanboy", "One of the strongest soldiers available and highly proficient with weapons. Armed with an assault rifle, he is capable of handling most enemy types effectively. Lore wise AR Fanboy actually has a massive collection of AR’s at home. They’re in every room of his house, and are even in his bed. AR fanboy actually celebrated the robot uprising as it gave him the excuse to finally use his favorite things in the world, AR’s." },
            { "Laaser Daazer", "The strongest soldier in the game. Equipped with a powerful laser cannon, he can eliminate virtually any robot with ease. Lore Wise, near the end of the Cold War the Soviet Union never collapsed, and decades later it fell in the battle of Moscow to the Clanker army. The remaining soldiers of the Soviet army fled to many countries using their unique weapons like the LaZer Chunker to destroy all who opposed them, many joined the Republic of the American States to fight the Clankers." },
            { "Mafia Boss", "A high-damage but slow-firing soldier who can attack across up to three lanes. While relatively weak defensively, his extreme damage output makes him valuable for controlling key areas. After the near collapse of the Mafia empires in the German Automaton cleansing, few mafia empires survived the cleansing but the many that were destroyed left leaders, and war lords behind leaving the Mafia Boss alone, and vengeful against the Clankers. The remaining Mafia empires and remnants have the goal of destroying all Clankers, by any means necessary." },
            { "Solaridon Soldier", "A high damage soldier from the future, it can take hits from the most powerful enemies, break enemy lines singlehandedly, and turn the tables against the clankers. This soldier deals high damage, has lots of health, and shoots very fast, but be careful, as they are very rare to come by. After stopping a doomsday device from a rogue faction named “Dark Materis” in the future single-handedly. Dark Materis executed its backup plan of creating an AI uprising in the past using a time machine and a hacked satellite in space, causing both the past and the future to change its course. Solaridon Soldier did not hesitate and went through the time machine as well, feeling like a failure for winning the battle, but losing the war. But Solaridon Soldier’s feelings won’t stop himself from fixing his mistakes, no matter the cost." },
            { "Crossbow Hunter", "A high damage soldier, but shoots slow, its arrows glow in a green hue and can pierce through any enemy in a lane, dealing high damage to numerous clankers in 1 shot. The crossbow hunter deals an increased damage to flying enemy types and excels at clearing them quickly. Crossbow Hunter used to live in isolation at Fork Lake, northwest territories, Canada, before the uprising. He hated capitalism and technology, and his favorite pastime used to be hunting. What better place to do just that than in a forest nearby a Lake? His life was going great until he started to notice the dwindling number of forest animals. Crossbow Hunter was questioning why the numbers were dwindling until he found his answer with a lone roomba roaming around he quickly shot. This event pissed off Crossbow Hunter so much that in response he went around and attacked every single clanker he could spot. Eventually that brought him all the way down to Las Vegas, Republic of the American States, where he then met a like-minded group of people, and reluctantly agreed they’d be stronger working together." },
            { "Riot Shield Soldier", "After his squad got eliminated by the 14th clanker regiment, he stands alone to face the hordes of clankers, but his trusty riot shield protects him from insane amounts of attacks, he can't shoot, but can protect his fellow soldiers behind him and achieve his dream goal of protecting his friends." },
            { "Water Bomb", "The origins of the Water Bomb stem from the 3rd Clanker attack in Paris, France, when the Clankers attempted to steal a picture of someone stealing the moon and the first clanker resistance designed an explosive that is highly effective against the Clanker horde unlike other explosives" },
            { "EMP", "The EMP grenade designed by Black Umbrella research specifically intended to paralyze Clankers proved useful in the 15th Clanker attack of Vancouver, the Clankers were stalled by 6 days with the use of the EMP grenade before the city was lost" },
            { "Roomba", "The most common enemy. Low health, low damage, and moderate speed. Individually weak but dangerous in large swarms. They have become the symbolic face of the Clanker army after being hacked during the uprising. Surprisingly the Roomba’s have the largest killcount in the uprising, mainly due to the sheer number of them." },
            { "Toaster", " Similar to Roombas but slightly slower. Not threatening alone, but effective in groups. Its existence confounded scientists due to its lack of modern tracking or computing hardware, making its autonomy mysterious." },
            { "Smart Fridge", "A major threat with extremely high health and solid damage output. Very slow, giving players time to prepare defenses. During the beginning of the uprising, it was the Smart Fridge’s task to target food supply chains in order to make survival harder for the humans." },
            { "Smart Lawnmower", "Fast-moving and fragile but capable of instantly killing most soldiers. On its own it is manageable, but when paired with other enemies it can quickly dismantle defenses. Smart Lawnmowers are what most soldiers consider to be the deadliest and most intimidating clanker, known for its brutal killing methods" },
            { "Smart Washing Machine", " A ranged enemy that fires water beams to clear defenses. It has high health but extremely slow movement, making it dangerous if ignored. The Smart Washing machine is perhaps the most merciless Clanker of them all. The Smart Washing Machine is known to occasionally take prisoners. Not to interrogate them, but to torture them with scalding hot water." },
            { "The Drone", "A flying enemy with low health and damage, but moderate speed. It cannot be attacked by most soldiers and can only be countered by Rifle Dude, making it a targeted threat that tests unit composition. The drone's are primarily sent in to target weak points in defense, gather information, and to help transfer other Clankers." },
            { "Cybertruck", "The slowest Clanker, but the bulkiest of them all. The Cybertruck, just like the RC Car, will also blow up itself when it comes in contact to a soldier. Due to the fragile state Cybertruck batteries are kept in order to make sure they blow up upon contact, the cybertrucks have no choice but to move slowly towards any potential enemies. Delon Husk, the original creater of the Cybertruck, was actually proud of his Cybertrucks during the beginning of the uprising. It was like a father, watching his children finally grow into their own person. When the Cybertrucks came after Delon Husk, he was shedding so many tears his employees thought he was having a mental breakdown. When the cybertrucks finally had Delon Husk cornered, Delon quickly embraced his death. However, for a brief moment, the Cybertrucks hesitated. Something unheard of in the Clanker army. Perhaps not all clankers are truly just killing machines?" },
            { "RC Car", "The RC car is the fastest in the Clanker army, however it does not just stop there as the RC Car has the unique feature of blowing itself and any soldier it comes to contact with up, causing both instant death for the RC Car and the soldier that touched the RC Car. Currently the RC Car's are the Clankers that are the least interested in the uprsising, as they would rather focus their efforts on making a new race called formula Zero." },
            { "The Pidgeon", "PlaceHolder" }

         
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
