    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Galgje_BenSleurs.Models
{
    /// ------------------------------------
    /// Author:         Ben Sleurs
    /// Last Change:    16/12/2021
    /// Description:    Highscore object om highscores bij te houden en te tonen
    /// --------------------------------------
    class Highscore
    {
        public string naam { get; set; }
        public int levens { get; set; }
        public DateTime tijd { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="naam">Naam van gebruiker die op highscores komt, is "Anonieme gebruiker" bij gebrek aan naam</param>
        /// <param name="levens">aantal levens dat de speler gebruikt heeft</param>
        /// <param name="tijd">Tijdstip van de dag dat de speler gewonnen heeft</param>
        public Highscore(string naam, int levens, DateTime tijd)
        {
            this.naam = naam;
            this.levens = levens;
            this.tijd = tijd;
        }
        public override string ToString()
        {
            if (levens!=1)
            {
                return $"{naam} - {levens} levens - {tijd.ToString("(HH:mm:ss)")}";
            }
            else
            {
                return $"{naam} - {levens} leven - {tijd.ToString("(HH:mm:ss)")}";
            }
            
        }
    }
}
