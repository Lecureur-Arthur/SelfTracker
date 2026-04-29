using UnityEngine;
using UnityEngine.UIElements;
using System;
using Unity.Notifications.Android;
using UnityEngine.Android;
using System.Globalization;

public class CantineManager : MonoBehaviour
{
    private UIDocument monUI;

    // Variables métier
    private float monSolde = 0f;
    private float prixDUnRepas = 3.10f;

    // UI Références
    private Label labelSolde, labelRepasRestants, labelEstimation, labelQuestion;
    private TextField inputRecharge, inputPrixRepas;
    private VisualElement panneauConfirmation, panneauSettings, panneauConfirmationReset;
    
    private VisualElement panneauCalendrier, grilleJours;
    private Label labelMoisAnnee;
    private Button btnOuvrirCalendrier;
    private DateTime dateAfficheeCalendrier, dateSelectionneeEnCours = DateTime.MinValue; 

    private string[] nomsDesMois = { "Janvier", "Février", "Mars", "Avril", "Mai", "Juin", "Juillet", "Août", "Septembre", "Octobre", "Novembre", "Décembre" };

    void OnEnable()
    {
        if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");

        monUI = GetComponent<UIDocument>();
        var root = monUI.rootVisualElement;

        // Liaison Éléments UI
        labelSolde = root.Q<Label>("TexteSolde");
        labelRepasRestants = root.Q<Label>("TexteRepasRestants");
        labelEstimation = root.Q<Label>("TexteEstimation");
        labelQuestion = root.Q<Label>("TexteQuestion");
        inputRecharge = root.Q<TextField>("ChampRecharge");
        inputPrixRepas = root.Q<TextField>("ChampPrixRepas");
        
        panneauConfirmation = root.Q<VisualElement>("PanneauConfirmation");
        panneauCalendrier = root.Q<VisualElement>("PanneauCalendrier");
        panneauSettings = root.Q<VisualElement>("PanneauSettings");
        panneauConfirmationReset = root.Q<VisualElement>("PanneauConfirmationReset");
        grilleJours = root.Q<VisualElement>("GrilleJours");
        labelMoisAnnee = root.Q<Label>("TexteMoisAnnee");
        btnOuvrirCalendrier = root.Q<Button>("BoutonOuvrirCalendrier");

        // Événements boutons
        root.Q<Button>("BoutonAjouter").clicked += AjouterArgent;
        root.Q<Button>("BoutonPrendreRepas").clicked += OuvrirPopupRepas;
        root.Q<Button>("BoutonConfirmer").clicked += ConfirmerRepas;
        root.Q<Button>("BoutonAnnuler").clicked += () => panneauConfirmation.style.display = DisplayStyle.None;

        btnOuvrirCalendrier.clicked += OuvrirLeCalendrier;
        root.Q<Button>("BoutonMoisPrec").clicked += () => ChangerMois(-1);
        root.Q<Button>("BoutonMoisSuiv").clicked += () => ChangerMois(1);
        root.Q<Button>("BoutonFermerCalendrier").clicked += () => panneauCalendrier.style.display = DisplayStyle.None;
        root.Q<Button>("BoutonValiderCalendrier").clicked += ValiderLeCalendrier;

        root.Q<Button>("BoutonOuvrirSettings").clicked += () => panneauSettings.style.display = DisplayStyle.Flex;
        root.Q<Button>("BoutonFermerSettings").clicked += () => panneauSettings.style.display = DisplayStyle.None;
        
        root.Q<Button>("BoutonOuvrirConfirmationReset").clicked += () => panneauConfirmationReset.style.display = DisplayStyle.Flex;
        root.Q<Button>("BoutonAnnulerReset").clicked += () => panneauConfirmationReset.style.display = DisplayStyle.None;
        root.Q<Button>("BoutonConfirmerReset").clicked += ResetLeSolde;

        // Callback changement prix
        inputPrixRepas.RegisterValueChangedCallback(evt => {
            string textPropre = evt.newValue.Replace(',', '.'); 
            if (float.TryParse(textPropre, NumberStyles.Any, CultureInfo.InvariantCulture, out float p)) {
                prixDUnRepas = p;
                PlayerPrefs.SetFloat("PrixRepas", prixDUnRepas);
                MettreAJourAffichage();
            }
        });

        // Init
        monSolde = PlayerPrefs.GetFloat("MonSolde", 0f);
        prixDUnRepas = PlayerPrefs.GetFloat("PrixRepas", 3.10f);
        inputPrixRepas.value = prixDUnRepas.ToString("F2", CultureInfo.InvariantCulture);

        MettreAJourAffichage();
        InitialiserCanalNotifications();
        GererNotificationsQuotidiennes();
    }

    // --- LOGIQUE CALENDRIER ---

    void OuvrirLeCalendrier()
    {
        dateAfficheeCalendrier = DateTime.Today;
        dateSelectionneeEnCours = DateTime.MinValue; 
        GenererJoursDuCalendrier();
        panneauCalendrier.style.display = DisplayStyle.Flex;
    }

    void ChangerMois(int direction)
    {
        dateAfficheeCalendrier = dateAfficheeCalendrier.AddMonths(direction);
        GenererJoursDuCalendrier();
    }

    int CalculerJoursOuvrables(DateTime debut, DateTime fin)
    {
        int count = 0;
        for (DateTime date = debut.Date; date <= fin.Date; date = date.AddDays(1))
        {
            if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                count++;
        }
        return count;
    }

    void GenererJoursDuCalendrier()
    {
        labelMoisAnnee.text = $"{nomsDesMois[dateAfficheeCalendrier.Month - 1]} {dateAfficheeCalendrier.Year}";
        grilleJours.Clear();

        DateTime premierJour = new DateTime(dateAfficheeCalendrier.Year, dateAfficheeCalendrier.Month, 1);
        int nbJoursDansMois = DateTime.DaysInMonth(dateAfficheeCalendrier.Year, dateAfficheeCalendrier.Month);
        int decallage = (int)premierJour.DayOfWeek - 1;
        if (decallage < 0) decallage = 6; 

        int repasDisponiblesActuellement = Mathf.FloorToInt(monSolde / prixDUnRepas);

        for (int i = 0; i < 42; i++)
        {
            Button boutonJour = new Button();
            boutonJour.style.width = Length.Percent(14.2f); 
            boutonJour.style.height = 100; // ENCORE PLUS HAUT !
            boutonJour.style.marginRight = 0; boutonJour.style.marginLeft = 0; 
            boutonJour.style.fontSize = 32; // ENCORE PLUS GROS !
            boutonJour.style.color = new StyleColor(Color.white);
            
            boutonJour.style.borderTopWidth = 0;
            boutonJour.style.borderBottomWidth = 0;
            boutonJour.style.borderLeftWidth = 0;
            boutonJour.style.borderRightWidth = 0;

            int jourActuel = i - decallage + 1;

            if (jourActuel > 0 && jourActuel <= nbJoursDansMois)
            {
                boutonJour.text = jourActuel.ToString();
                DateTime dateDuBouton = new DateTime(dateAfficheeCalendrier.Year, dateAfficheeCalendrier.Month, jourActuel);
                boutonJour.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));

                if (dateDuBouton.Date < DateTime.Today.Date)
                {
                    boutonJour.SetEnabled(false);
                    boutonJour.style.color = new StyleColor(Color.gray);
                }
                else if (dateDuBouton.DayOfWeek == DayOfWeek.Saturday || dateDuBouton.DayOfWeek == DayOfWeek.Sunday)
                {
                    boutonJour.style.color = new StyleColor(Color.gray);
                    boutonJour.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f));
                    boutonJour.SetEnabled(false);
                }
                else 
                {
                    int repasRequis = CalculerJoursOuvrables(DateTime.Today, dateDuBouton);

                    if (repasRequis <= repasDisponiblesActuellement)
                        boutonJour.style.backgroundColor = new StyleColor(new Color(0.3f, 0.69f, 0.31f)); // Vert
                    else if (dateSelectionneeEnCours != DateTime.MinValue && dateDuBouton.Date <= dateSelectionneeEnCours.Date)
                        boutonJour.style.backgroundColor = new StyleColor(new Color(1f, 0.6f, 0f)); // Orange

                    boutonJour.clicked += () => {
                        dateSelectionneeEnCours = dateDuBouton;
                        GenererJoursDuCalendrier(); 
                    };
                }
            }
            else
            {
                boutonJour.text = "";
                boutonJour.style.backgroundColor = new StyleColor(Color.clear); 
                boutonJour.SetEnabled(false);
            }
            grilleJours.Add(boutonJour);
        }
    }

    void ValiderLeCalendrier()
    {
        if (dateSelectionneeEnCours == DateTime.MinValue)
        {
            panneauCalendrier.style.display = DisplayStyle.None;
            return; 
        }

        panneauCalendrier.style.display = DisplayStyle.None;
        btnOuvrirCalendrier.text = "Jusqu'au " + dateSelectionneeEnCours.ToString("dd/MM/yyyy");

        int joursDeSemaine = CalculerJoursOuvrables(DateTime.Today, dateSelectionneeEnCours);
        float totalBesoin = joursDeSemaine * prixDUnRepas;
        float manque = totalBesoin - monSolde;
        
        if (manque <= 0) 
        {
            labelEstimation.text = "Vous avez déjà assez d'argent !";
            labelEstimation.style.color = new StyleColor(new Color(0.3f, 0.69f, 0.31f)); 
        }
        else 
        {
            labelEstimation.text = $"Pour {joursDeSemaine} repas, rajouter : {manque:F2}€";
            labelEstimation.style.color = new StyleColor(new Color(1f, 0.6f, 0f)); 
        }
    }

    // --- LOGIQUE CLASSIQUE ---

    void AjouterArgent()
    {
        string textePropre = inputRecharge.value.Replace(',', '.');
        if (float.TryParse(textePropre, NumberStyles.Any, CultureInfo.InvariantCulture, out float montant))
        {
            monSolde += montant;
            inputRecharge.value = "";
            Sauvegarder();
            if (dateSelectionneeEnCours != DateTime.MinValue) ValiderLeCalendrier();
        }
    }

    void OuvrirPopupRepas()
    {
        labelQuestion.text = $"Soustraire {prixDUnRepas:F2}€ pour un repas ?";
        panneauConfirmation.style.display = DisplayStyle.Flex;
    }

    void ConfirmerRepas()
    {
        monSolde -= prixDUnRepas;
        panneauConfirmation.style.display = DisplayStyle.None;
        Sauvegarder();
        if (dateSelectionneeEnCours != DateTime.MinValue) ValiderLeCalendrier();
    }

    void ResetLeSolde()
    {
        monSolde = 0;
        panneauConfirmationReset.style.display = DisplayStyle.None;
        panneauSettings.style.display = DisplayStyle.None;
        Sauvegarder();
        if (dateSelectionneeEnCours != DateTime.MinValue) ValiderLeCalendrier();
    }

    void MettreAJourAffichage()
    {
        labelSolde.text = $"Solde : {monSolde:F2}€";
        int nbRepas = Mathf.FloorToInt(monSolde / prixDUnRepas);
        labelRepasRestants.text = $"Repas restants : {nbRepas}";
    }

    void Sauvegarder()
    {
        PlayerPrefs.SetFloat("MonSolde", monSolde);
        PlayerPrefs.Save();
        MettreAJourAffichage();
    }

    // --- NOTIFICATIONS ---

    void InitialiserCanalNotifications()
    {
        var canal = new AndroidNotificationChannel()
        {
            Id = "canal_cantine",
            Name = "Rappels Cantine",
            Importance = Importance.High,
            Description = "Demande si j'ai mangé",
        };
        AndroidNotificationCenter.RegisterNotificationChannel(canal);
    }

    void GererNotificationsQuotidiennes()
    {
        AndroidNotificationCenter.CancelAllDisplayedNotifications();
        AndroidNotificationCenter.CancelAllScheduledNotifications();

        for (int i = 0; i < 7; i++)
        {
            DateTime dateNotif = DateTime.Today.AddDays(i).AddHours(12).AddMinutes(15);
            if (dateNotif < DateTime.Now) continue;

            if (dateNotif.DayOfWeek != DayOfWeek.Saturday && dateNotif.DayOfWeek != DayOfWeek.Sunday)
            {
                var notification = new AndroidNotification();
                notification.Title = "C'est l'heure de manger !";
                notification.Text = "As-tu pris un repas à la cantine aujourd'hui ?";
                notification.FireTime = dateNotif;
                notification.SmallIcon = "icon_0";
                AndroidNotificationCenter.SendNotification(notification, "canal_cantine");
            }
        }
    }
}