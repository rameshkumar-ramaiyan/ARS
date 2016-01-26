using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace USDA_ARS.Umbraco.Extensions.Helpers
{
    public class SelectList
    {
        public static List<SelectListItem> StateList()
        {
            List<SelectListItem> stateList = new List<SelectListItem>();

            stateList.Add(new SelectListItem { Text = "Select...", Value = "none" });
            stateList.Add(new SelectListItem { Text = "Alabama", Value = "Alabama" });
            stateList.Add(new SelectListItem { Text = "Alaska", Value = "Alaska" });
            stateList.Add(new SelectListItem { Text = "Arizona", Value = "Arizona" });
            stateList.Add(new SelectListItem { Text = "Arkansas", Value = "Arkansas" });
            stateList.Add(new SelectListItem { Text = "California", Value = "California" });
            stateList.Add(new SelectListItem { Text = "Colorado", Value = "Colorado" });
            stateList.Add(new SelectListItem { Text = "Connecticut", Value = "Connecticut" });
            stateList.Add(new SelectListItem { Text = "Delaware", Value = "Delaware" });
            stateList.Add(new SelectListItem { Text = "Florida", Value = "Florida" });
            stateList.Add(new SelectListItem { Text = "Georgia", Value = "Georgia" });
            stateList.Add(new SelectListItem { Text = "Hawaii", Value = "Hawaii" });
            stateList.Add(new SelectListItem { Text = "Idaho", Value = "Idaho" });
            stateList.Add(new SelectListItem { Text = "Illinois", Value = "Illinois" });
            stateList.Add(new SelectListItem { Text = "Indiana", Value = "Indiana" });
            stateList.Add(new SelectListItem { Text = "Iowa", Value = "Iowa" });
            stateList.Add(new SelectListItem { Text = "Kansas", Value = "Kansas" });
            stateList.Add(new SelectListItem { Text = "Kentucky", Value = "Kentucky" });
            stateList.Add(new SelectListItem { Text = "Louisiana", Value = "Louisiana" });
            stateList.Add(new SelectListItem { Text = "Maine", Value = "Maine" });
            stateList.Add(new SelectListItem { Text = "Maryland", Value = "Maryland" });
            stateList.Add(new SelectListItem { Text = "Massachusetts", Value = "Massachusetts" });
            stateList.Add(new SelectListItem { Text = "Michigan", Value = "Michigan" });
            stateList.Add(new SelectListItem { Text = "Minnesota", Value = "Minnesota" });
            stateList.Add(new SelectListItem { Text = "Mississippi", Value = "Mississippi" });
            stateList.Add(new SelectListItem { Text = "Missouri", Value = "Missouri" });
            stateList.Add(new SelectListItem { Text = "Montana", Value = "Montana" });
            stateList.Add(new SelectListItem { Text = "Nebraska", Value = "Nebraska" });
            stateList.Add(new SelectListItem { Text = "Nevada", Value = "Nevada" });
            stateList.Add(new SelectListItem { Text = "New Hampshire", Value = "New Hampshire" });
            stateList.Add(new SelectListItem { Text = "New Jersey", Value = "New Jersey" });
            stateList.Add(new SelectListItem { Text = "New Mexico", Value = "New Mexico" });
            stateList.Add(new SelectListItem { Text = "New York", Value = "New York" });
            stateList.Add(new SelectListItem { Text = "North Carolina", Value = "North Carolina" });
            stateList.Add(new SelectListItem { Text = "North Dakota", Value = "North Dakota" });
            stateList.Add(new SelectListItem { Text = "Ohio", Value = "Ohio" });
            stateList.Add(new SelectListItem { Text = "Oklahoma", Value = "Oklahoma" });
            stateList.Add(new SelectListItem { Text = "Oregon", Value = "Oregon" });
            stateList.Add(new SelectListItem { Text = "Pennsylvania", Value = "Pennsylvania" });
            stateList.Add(new SelectListItem { Text = "Rhode Island", Value = "Rhode Island" });
            stateList.Add(new SelectListItem { Text = "South Carolina", Value = "South Carolina" });
            stateList.Add(new SelectListItem { Text = "South Dakota", Value = "South Dakota" });
            stateList.Add(new SelectListItem { Text = "Tennessee", Value = "Tennessee" });
            stateList.Add(new SelectListItem { Text = "Texas", Value = "Texas" });
            stateList.Add(new SelectListItem { Text = "U.S. Territories", Value = "U.S. Territories" });
            stateList.Add(new SelectListItem { Text = "Utah", Value = "Utah" });
            stateList.Add(new SelectListItem { Text = "Vermont", Value = "Vermont" });
            stateList.Add(new SelectListItem { Text = "Virginia", Value = "Virginia" });
            stateList.Add(new SelectListItem { Text = "Washington", Value = "Washington" });
            stateList.Add(new SelectListItem { Text = "Washington D.C.", Value = "Washington D.C." });
            stateList.Add(new SelectListItem { Text = "West Virginia", Value = "West Virginia" });
            stateList.Add(new SelectListItem { Text = "Wisconsin", Value = "Wisconsin" });
            stateList.Add(new SelectListItem { Text = "Wyoming", Value = "Wyoming" });

            return stateList;
        }


        public static List<SelectListItem> CountryList()
        {
            List<SelectListItem> countryList = new List<SelectListItem>();

            countryList.Add(new SelectListItem { Text = "Select...", Value = "none" });
            countryList.Add(new SelectListItem { Text = "United States", Value = "United States" });
            countryList.Add(new SelectListItem { Text = "Afganistan", Value = "Afghanistan" });
            countryList.Add(new SelectListItem { Text = "Albania", Value = "Albania" });
            countryList.Add(new SelectListItem { Text = "Algeria", Value = "Algeria" });
            countryList.Add(new SelectListItem { Text = "Andorra", Value = "Andorra" });
            countryList.Add(new SelectListItem { Text = "Angola", Value = "Angola" });
            countryList.Add(new SelectListItem { Text = "Anitgua and Barbuda", Value = "Antiqua and Barbuda" });
            countryList.Add(new SelectListItem { Text = "Argentina", Value = "Argentina" });
            countryList.Add(new SelectListItem { Text = "Armenia", Value = "Armenia" });
            countryList.Add(new SelectListItem { Text = "Australia", Value = "Australia" });
            countryList.Add(new SelectListItem { Text = "Azerbaijan", Value = "Azerbaijan" });
            countryList.Add(new SelectListItem { Text = "Bahamas", Value = "Bahamas" });
            countryList.Add(new SelectListItem { Text = "Bahrain", Value = "Bahrain" });
            countryList.Add(new SelectListItem { Text = "Bangladesh", Value = "Bangladesh" });
            countryList.Add(new SelectListItem { Text = "Barbados", Value = "Barbados" });
            countryList.Add(new SelectListItem { Text = "Belarus", Value = "Belarus" });
            countryList.Add(new SelectListItem { Text = "Belgium", Value = "Belgium" });
            countryList.Add(new SelectListItem { Text = "Belize", Value = "Belize" });
            countryList.Add(new SelectListItem { Text = "Benin", Value = "Benin" });
            countryList.Add(new SelectListItem { Text = "Bhutan", Value = "Bhutan" });
            countryList.Add(new SelectListItem { Text = "Bolivia", Value = "Bolivia" });
            countryList.Add(new SelectListItem { Text = "Bosnia and Herzegovina", Value = "Bosnia and Herzegovina" });
            countryList.Add(new SelectListItem { Text = "Botswana", Value = "Botswana" });
            countryList.Add(new SelectListItem { Text = "Brazil", Value = "Brazil" });
            countryList.Add(new SelectListItem { Text = "Brunei", Value = "Brunei" });
            countryList.Add(new SelectListItem { Text = "Bulgaria", Value = "Bulgaria" });
            countryList.Add(new SelectListItem { Text = "Burkina Faso", Value = "Burkina Faso" });
            countryList.Add(new SelectListItem { Text = "Burundi", Value = "Burundi" });
            countryList.Add(new SelectListItem { Text = "Cambodia", Value = "Cambodia" });
            countryList.Add(new SelectListItem { Text = "Cameroon", Value = "Cameroon" });
            countryList.Add(new SelectListItem { Text = "Canada", Value = "Canada" });
            countryList.Add(new SelectListItem { Text = "Cape Verde", Value = "Cape Verde" });
            countryList.Add(new SelectListItem { Text = "Central African Republic", Value = "Central African Republic" });
            countryList.Add(new SelectListItem { Text = "Chad", Value = "Chad" });
            countryList.Add(new SelectListItem { Text = "Chile", Value = "Chile" });
            countryList.Add(new SelectListItem { Text = "China", Value = "China" });
            countryList.Add(new SelectListItem { Text = "Colombia", Value = "Colombia" });
            countryList.Add(new SelectListItem { Text = "Comoros", Value = "Comoros" });
            countryList.Add(new SelectListItem { Text = "Congo (Brazzaville)", Value = "Congo (Brazzaville)" });
            countryList.Add(new SelectListItem { Text = "Congo, Democratic Republic of the", Value = "Congo, Democratic  Republic of the" });
            countryList.Add(new SelectListItem { Text = "Costa Rica", Value = "Costa Rica" });
            countryList.Add(new SelectListItem { Text = "Croatia", Value = "Croatia" });
            countryList.Add(new SelectListItem { Text = "Cuba", Value = "Cuba" });
            countryList.Add(new SelectListItem { Text = "Cyprus", Value = "Cyprus" });
            countryList.Add(new SelectListItem { Text = "Czech Republic", Value = "Czech Republic" });
            countryList.Add(new SelectListItem { Text = "Cote d'Ivoire", Value = "Cote d'Ivoire" });
            countryList.Add(new SelectListItem { Text = "Denmark", Value = "Denmark" });
            countryList.Add(new SelectListItem { Text = "Djibouti", Value = "Djibouti" });
            countryList.Add(new SelectListItem { Text = "Dominica", Value = "Dominica" });
            countryList.Add(new SelectListItem { Text = "Dominican Republic", Value = "Dominican Republic" });
            countryList.Add(new SelectListItem { Text = "East Timor", Value = "East Timor" });
            countryList.Add(new SelectListItem { Text = "Ecuador", Value = "Ecuador" });
            countryList.Add(new SelectListItem { Text = "Egypt", Value = "Egypt" });
            countryList.Add(new SelectListItem { Text = "El Salvador", Value = "El Salvador" });
            countryList.Add(new SelectListItem { Text = "Equatorial Guinea", Value = "Equatorial Guinea" });
            countryList.Add(new SelectListItem { Text = "Eritrea", Value = "Eritrea" });
            countryList.Add(new SelectListItem { Text = "Estonia", Value = "Estonia" });
            countryList.Add(new SelectListItem { Text = "Ethiopia", Value = "Ethiopia" });
            countryList.Add(new SelectListItem { Text = "Fiji", Value = "Fiji" });
            countryList.Add(new SelectListItem { Text = "Finland", Value = "Finland" });
            countryList.Add(new SelectListItem { Text = "France", Value = "France" });
            countryList.Add(new SelectListItem { Text = "Gabon", Value = "Gabon" });
            countryList.Add(new SelectListItem { Text = "Gambia, The", Value = "Gambia, The" });
            countryList.Add(new SelectListItem { Text = "Georgia", Value = "Georgia" });
            countryList.Add(new SelectListItem { Text = "Germany", Value = "Germany" });
            countryList.Add(new SelectListItem { Text = "Ghana", Value = "Ghana" });
            countryList.Add(new SelectListItem { Text = "Greece", Value = "Greece" });
            countryList.Add(new SelectListItem { Text = "Grenada", Value = "Grenada" });
            countryList.Add(new SelectListItem { Text = "Guatemala", Value = "Guatemala" });
            countryList.Add(new SelectListItem { Text = "Guinea", Value = "Guinea" });
            countryList.Add(new SelectListItem { Text = "Guinea-Bissau", Value = "Guinea-Bissau" });
            countryList.Add(new SelectListItem { Text = "Guyana", Value = "Guyana" });
            countryList.Add(new SelectListItem { Text = "Haiti", Value = "Haiti" });
            countryList.Add(new SelectListItem { Text = "Honduras", Value = "Honduras" });
            countryList.Add(new SelectListItem { Text = "Hungary", Value = "Hungary" });
            countryList.Add(new SelectListItem { Text = "Iceland", Value = "Iceland" });
            countryList.Add(new SelectListItem { Text = "India", Value = "India" });
            countryList.Add(new SelectListItem { Text = "Indonesia", Value = "Indonesia" });
            countryList.Add(new SelectListItem { Text = "Iran", Value = "Iran" });
            countryList.Add(new SelectListItem { Text = "Iraq", Value = "Iraq" });
            countryList.Add(new SelectListItem { Text = "Ireland", Value = "Ireland" });
            countryList.Add(new SelectListItem { Text = "Israel", Value = "Israel" });
            countryList.Add(new SelectListItem { Text = "Italy", Value = "Italy" });
            countryList.Add(new SelectListItem { Text = "Jamaica", Value = "Jamaica" });
            countryList.Add(new SelectListItem { Text = "Japan", Value = "Japan" });
            countryList.Add(new SelectListItem { Text = "Jordan", Value = "Jordan" });
            countryList.Add(new SelectListItem { Text = "Kazakhstan", Value = "Kazakhstan" });
            countryList.Add(new SelectListItem { Text = "Kenya", Value = "Kenya" });
            countryList.Add(new SelectListItem { Text = "Kiribati", Value = "Kiribati" });
            countryList.Add(new SelectListItem { Text = "Kuwait", Value = "Kuwait" });
            countryList.Add(new SelectListItem { Text = "Kyrgyzstan", Value = "Kyrgyzstan" });
            countryList.Add(new SelectListItem { Text = "Laos", Value = "Laos" });
            countryList.Add(new SelectListItem { Text = "Latvia", Value = "Latvia" });
            countryList.Add(new SelectListItem { Text = "Lebanon", Value = "Lebanon" });
            countryList.Add(new SelectListItem { Text = "Lesotho", Value = "Lesotho" });
            countryList.Add(new SelectListItem { Text = "Liberia", Value = "Liberia" });
            countryList.Add(new SelectListItem { Text = "Libya", Value = "Libya" });
            countryList.Add(new SelectListItem { Text = "Liechtenstein", Value = "Liechtenstein" });
            countryList.Add(new SelectListItem { Text = "Lithuania", Value = "Lithuania" });
            countryList.Add(new SelectListItem { Text = "Luxembourg", Value = "Luxembourg" });
            countryList.Add(new SelectListItem { Text = "Macedonia", Value = "Macedonia" });
            countryList.Add(new SelectListItem { Text = "Madagascar", Value = "Madagascar" });
            countryList.Add(new SelectListItem { Text = "Malawi", Value = "Malawi" });
            countryList.Add(new SelectListItem { Text = "Malaysia", Value = "Malaysia" });
            countryList.Add(new SelectListItem { Text = "Maldives", Value = "Maldives" });
            countryList.Add(new SelectListItem { Text = "Mali", Value = "Mali" });
            countryList.Add(new SelectListItem { Text = "Malta", Value = "Malta" });
            countryList.Add(new SelectListItem { Text = "Marshall Islands", Value = "Marshall Islands" });
            countryList.Add(new SelectListItem { Text = "Maritania", Value = "Mauritania" });
            countryList.Add(new SelectListItem { Text = "Mauritius", Value = "Mauritius" });
            countryList.Add(new SelectListItem { Text = "Mexico", Value = "Mexico" });
            countryList.Add(new SelectListItem { Text = "Micronesia, Federated States of", Value = "Micronesia, Federated  States of" });
            countryList.Add(new SelectListItem { Text = "Moldova", Value = "Moldova" });
            countryList.Add(new SelectListItem { Text = "Monaco", Value = "Monaco" });
            countryList.Add(new SelectListItem { Text = "Mongolia", Value = "Mongolia" });
            countryList.Add(new SelectListItem { Text = "Morocco", Value = "Morocco" });
            countryList.Add(new SelectListItem { Text = "Mozambique", Value = "Mozambique" });
            countryList.Add(new SelectListItem { Text = "Myanmar", Value = "Myanmar" });
            countryList.Add(new SelectListItem { Text = "Namibia", Value = "Namibia" });
            countryList.Add(new SelectListItem { Text = "Nauru", Value = "Nauru" });
            countryList.Add(new SelectListItem { Text = "Nepal", Value = "Nepal" });
            countryList.Add(new SelectListItem { Text = "Netherlands", Value = "Netherlands" });
            countryList.Add(new SelectListItem { Text = "New Zealand", Value = "New Zealand" });
            countryList.Add(new SelectListItem { Text = "Nicaragua", Value = "Nicaragua" });
            countryList.Add(new SelectListItem { Text = "Niger", Value = "Niger" });
            countryList.Add(new SelectListItem { Text = "Nigeria", Value = "Nigeria" });
            countryList.Add(new SelectListItem { Text = "North Korea", Value = "North Korea" });
            countryList.Add(new SelectListItem { Text = "Norway", Value = "Norway" });
            countryList.Add(new SelectListItem { Text = "Oman", Value = "Oman" });
            countryList.Add(new SelectListItem { Text = "Pakistan", Value = "Pakistan" });
            countryList.Add(new SelectListItem { Text = "Palau", Value = "Palau" });
            countryList.Add(new SelectListItem { Text = "Panama", Value = "Panama" });
            countryList.Add(new SelectListItem { Text = "Papua New Guinea", Value = "Papua New Guinea" });
            countryList.Add(new SelectListItem { Text = "Paraguay", Value = "Paraguay" });
            countryList.Add(new SelectListItem { Text = "Peru", Value = "Peru" });
            countryList.Add(new SelectListItem { Text = "Philippines", Value = "Philippines" });
            countryList.Add(new SelectListItem { Text = "Poland", Value = "Poland" });
            countryList.Add(new SelectListItem { Text = "Portugal", Value = "Portugal" });
            countryList.Add(new SelectListItem { Text = "Qatar", Value = "Qatar" });
            countryList.Add(new SelectListItem { Text = "Romania", Value = "Romania" });
            countryList.Add(new SelectListItem { Text = "Russia", Value = "Russia" });
            countryList.Add(new SelectListItem { Text = "Rwanda", Value = "Rwanda" });
            countryList.Add(new SelectListItem { Text = "St. Kitts and St. Nevis", Value = "St. Kitts and St. Nevis" });
            countryList.Add(new SelectListItem { Text = "St. Lucia", Value = "St. Lucia" });
            countryList.Add(new SelectListItem { Text = "St. Vincent and The Grenadines", Value = "St. Vincent and The  Grenadines" });
            countryList.Add(new SelectListItem { Text = "Samoa", Value = "Samoa" });
            countryList.Add(new SelectListItem { Text = "San Marino", Value = "San Marino" });
            countryList.Add(new SelectListItem { Text = "Sao Tome and Principe", Value = "Sao Tome and Principe" });
            countryList.Add(new SelectListItem { Text = "Saudi Arabia", Value = "Saudi Arabia" });
            countryList.Add(new SelectListItem { Text = "Senegal", Value = "Senegal" });
            countryList.Add(new SelectListItem { Text = "Serbia and Montenegro", Value = "Serbia and Montenegro" });
            countryList.Add(new SelectListItem { Text = "Seychelles", Value = "Seychelles" });
            countryList.Add(new SelectListItem { Text = "Sierra Leone", Value = "Sierra Leone" });
            countryList.Add(new SelectListItem { Text = "Singapore", Value = "Singapore" });
            countryList.Add(new SelectListItem { Text = "Slovakia", Value = "Slovakia" });
            countryList.Add(new SelectListItem { Text = "Slovenia", Value = "Slovenia" });
            countryList.Add(new SelectListItem { Text = "Solomon Islands", Value = "Solomon Islands" });
            countryList.Add(new SelectListItem { Text = "Somalia", Value = "Somalia" });
            countryList.Add(new SelectListItem { Text = "South Africa", Value = "South Africa" });
            countryList.Add(new SelectListItem { Text = "South Korea", Value = "South Korea" });
            countryList.Add(new SelectListItem { Text = "Spain", Value = "Spain" });
            countryList.Add(new SelectListItem { Text = "Sri Lanka", Value = "Sri Lanka" });
            countryList.Add(new SelectListItem { Text = "Sudan", Value = "Sudan" });
            countryList.Add(new SelectListItem { Text = "Suriname", Value = "Suriname" });
            countryList.Add(new SelectListItem { Text = "Swaziland", Value = "Swaziland" });
            countryList.Add(new SelectListItem { Text = "Sweden", Value = "Sweden" });
            countryList.Add(new SelectListItem { Text = "Switzerland", Value = "Switzerland" });
            countryList.Add(new SelectListItem { Text = "Syria", Value = "Syria" });
            countryList.Add(new SelectListItem { Text = "Taiwan", Value = "Taiwan" });
            countryList.Add(new SelectListItem { Text = "Tajikistan", Value = "Tajikistan" });
            countryList.Add(new SelectListItem { Text = "Tanzania", Value = "Tanzania" });
            countryList.Add(new SelectListItem { Text = "Thailand", Value = "Thailand" });
            countryList.Add(new SelectListItem { Text = "Togo", Value = "Togo" });
            countryList.Add(new SelectListItem { Text = "Tonga", Value = "Tonga" });
            countryList.Add(new SelectListItem { Text = "Trinidad and Tobago", Value = "Trinidad and Tobago" });
            countryList.Add(new SelectListItem { Text = "Tunisia", Value = "Tunisia" });
            countryList.Add(new SelectListItem { Text = "Turkey", Value = "Turkey" });
            countryList.Add(new SelectListItem { Text = "Turkmenistan", Value = "Turkmenistan" });
            countryList.Add(new SelectListItem { Text = "Tuvalu", Value = "Tuvalu" });
            countryList.Add(new SelectListItem { Text = "Uganda", Value = "Uganda" });
            countryList.Add(new SelectListItem { Text = "Ukraine", Value = "Ukraine" });
            countryList.Add(new SelectListItem { Text = "United Arab Emirates", Value = "United Arab Emirates" });
            countryList.Add(new SelectListItem { Text = "United Kingdom", Value = "United Kingdom" });
            countryList.Add(new SelectListItem { Text = "Uruguay", Value = "Uruguay" });
            countryList.Add(new SelectListItem { Text = "Uzbekistan", Value = "Uzbekistan" });
            countryList.Add(new SelectListItem { Text = "Vanuatu", Value = "Vanuatu" });
            countryList.Add(new SelectListItem { Text = "Vatican City", Value = "Vatican City" });
            countryList.Add(new SelectListItem { Text = "Venezuela", Value = "Venezuela" });
            countryList.Add(new SelectListItem { Text = "Vietnam", Value = "Vietnam" });
            countryList.Add(new SelectListItem { Text = "Western Sahara", Value = "Western Sahara" });
            countryList.Add(new SelectListItem { Text = "Yemen", Value = "Yemen" });
            countryList.Add(new SelectListItem { Text = "Zambia", Value = "Zambia" });
            countryList.Add(new SelectListItem { Text = "Zimbabwe", Value = "Zimbabwe" });

            return countryList;
        }


        public static List<SelectListItem> ReferenceList()
        {
            List<SelectListItem> referenceList = new List<SelectListItem>();

            referenceList.Add(new SelectListItem { Text = "Select...", Value = "none" });
            referenceList.Add(new SelectListItem { Text = "Publication or Other", Value = "Publication or Other Source" });
            referenceList.Add(new SelectListItem { Text = "No Prior Knowledge", Value = "No Prior Knowledge" });
            referenceList.Add(new SelectListItem { Text = "Colleague", Value = "Colleague or Friend" });
            referenceList.Add(new SelectListItem { Text = "Upgrading", Value = "Upgrading from an Older Version" });

            return referenceList;
        }


        public static List<SelectListItem> PurposeList()
        {
            List<SelectListItem> purposeList = new List<SelectListItem>();

            purposeList.Add(new SelectListItem { Text = "Select...", Value = "none" });
            purposeList.Add(new SelectListItem { Text = "Field Research", Value = "Field Research" });
            purposeList.Add(new SelectListItem { Text = "Farming", Value = "Farming" });
            purposeList.Add(new SelectListItem { Text = "Software Development", Value = "Software Development" });
            purposeList.Add(new SelectListItem { Text = "Teaching", Value = "Teaching" });

            return purposeList;
        }
    }
}
