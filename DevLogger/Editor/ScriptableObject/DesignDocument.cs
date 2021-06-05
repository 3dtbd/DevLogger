using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.DevLogger
{
    /// <summary>
    /// A Design Document captures the basics of a game design and forms the central reference
    /// when making decisions about what does and does not go into the game itself.
    /// </summary>
    [CreateAssetMenu(fileName = "New Design Document", menuName = "Wizards Code/Design Document")]
    public class DesignDocument : ScriptableObject
    {
        [Tooltip("A single sentence description of the game that you will use to guide design decisions. (Example: Stylized action platformer about a meatball fighting the dinner table.)")]
        public string m_GameIdentity;

        [Tooltip("List up to 3 words/phrases that convey the feeling or emotion you want the player to experience. They are what the player will think of first when thinking of your game. (Example: Fast.Action - packed.Mayhem.)")]
        public string[] m_DesignPillars;

        [Tooltip("Describe the central process that is repeated through the game.This is the crux of the user experience. Identify the means of retention through rewards in the loop. (Example: Clash of Clans: Collect Resources, Build and Train Troops, Defeat Enemies)")]
        public string m_GameLoop;

        [Tooltip("3 to 5 specific points in gameplay that delight the player.They are shareable moments in the form of a story, they trigger emotion. Describe each moment in a single, very specific sentence. (Example: Mario Kart: Cross the finish line and the Kart zooms off into the distance. Hitting a player with a collectable and they are visibly delayed.Landing a perfect drift to get a major speed boost)")]
        public string[] m_MagicMoments;

        [Tooltip("List the cool features or unique elements that you want to include in your game.This is a complete list of all key features and what matters most for each feature. It helps form a holistic view of what the game needs to succeed and thus cannot be compromised and helps guide decision making during development. (Example: Grand Theft Auto: Intuitive and accessible map feature providing contextual information for the players immediate surroundings while driving or whole world during Garage time.Easily \"failure free\" driving system that provides fun way to move between missions but does not hamper the player with unnecessary challenges.")]
        public string[] m_Features;

        [Tooltip("Describe what makes your game great in just a few sentences. (Example: FIFA18: Authentic football action using the stars and teams that you know from around the world today.Unparalleled strategic realism lets you control your team with incredible precision.) Provide a single sentence that describes your vision for the game. (Example: FIFA18: Authentic football that makes the player the star.)")]
        public string m_ElevatorPitch;

        [Tooltip("List the player input method, the controls, and how the player interacts with your game.")]
        public string[] m_InterfaceAndControls;

        [Tooltip("Include references to images, models and games that have a similar aesthetic to what you're trying to achieve.")]
        public string[] m_ArtStyle;

        [Tooltip("Include links to music and sound design similar to what you're trying to achieve. You can also list the emotional responses that the sound should invoke in the player.")]
        public string[] m_MusicSound;

        [Tooltip("Which platforms is the game to be delivered on?")]
        public string[] m_TargetPlatforms;

    }
}