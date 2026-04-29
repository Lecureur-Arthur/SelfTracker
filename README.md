# SuiviSelf 

![Version](https://img.shields.io/badge/version-1.0.0-blue)
![Platform](https://img.shields.io/badge/platform-Android-green)
![Unity](https://img.shields.io/badge/Unity-6000.3.11f1-black?logo=unity)

SuiviSelf est une application Android que j'ai développée sur Unity pour gérer mon budget de cantine/self. L'idée est simple : arrêter de deviner combien il me reste sur ma carte et savoir précisément quand je vais devoir la recharger.

## Téléchargement et Installation

Le fichier APK est disponible directement dans la section **Releases** du projet.

> **[Télécharger la dernière version (APK)](https://github.com/Lecureur-Arthur/SelfTracker/releases/latest)**

1. Téléchargez le fichier sur votre téléphone.
2. Ouvrez l'APK pour lancer l'installation.
3. Si Android bloque l'installation, autorisez les "sources inconnues" dans vos paramètres de sécurité.

## Pourquoi ce projet ?

J'en avais marre de ne jamais être sûr de mon solde au moment de passer au self. J'ai donc créé cette application pour avoir un suivi instantané. L'interface est volontairement très large (format XL) pour être manipulée facilement à une main sur smartphone.

## Fonctionnalités

* **Suivi du solde :** Un bouton "J'ai mangé !" pour déduire le prix d'un repas instantanément.
* **Calendrier prévisionnel :** * Affiche en **vert** les jours couverts par le solde actuel.
    * Affiche en **orange** les jours sélectionnés pour recharge.
    * Calcule automatiquement le montant à ajouter (exclut les week-ends).
* **Menu Paramètres :** Changement du prix du repas et option de remise à zéro du solde sécurisée par une confirmation.
* **Rappels automatiques :** Notification chaque jour à 12h15 (Lundi-Vendredi) pour ne pas oublier de noter son repas.

## Infos techniques

* **Moteur :** Unity 6 (UI Toolkit).
* **Design :** Fichiers UXML pour la structure et USS pour les feuilles de style.
* **Données :** Persistance locale via PlayerPrefs.
* **Ergonomie :** Interface responsive, clavier numérique forcé (Decimal Pad) et gestion universelle point/virgule.

---
*Projet développé pour un usage personnel et comme exercice de développement mobile.*
