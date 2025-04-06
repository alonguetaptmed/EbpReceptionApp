# EBP Réception App

Application mobile Xamarin pour la gestion des réceptions fournisseurs avec intégration EBP.

## Description

Cette application vise à digitaliser et simplifier le processus de réception fournisseur. Elle permet de gérer la réception, la vérification et le rangement des marchandises via une interface tactile optimisée pour tablettes et smartphones Android.

L'application se connecte au serveur EBP via des webhooks n8n pour synchroniser les données.

## Fonctionnalités principales

- Consultation des commandes fournisseurs avec filtres par statut
- Réception totale ou partielle des marchandises
- Gestion des étiquettes et métrages linéaires
- Impression de documents (BL, étiquettes, etc.)
- Support multi-dépôts (Vallauris / Sainte Hermine)
- Interface utilisateur optimisée pour écrans tactiles

## Prérequis techniques

- Visual Studio 2022 ou supérieur
- Xamarin.Forms 5.0 ou supérieur
- Tablette ou téléphone Android avec accès réseau

## Architecture

L'application suit une architecture MVVM (Model-View-ViewModel) et utilise des services RESTful pour communiquer avec les webhooks n8n qui font l'interface avec le serveur EBP.

## Contributeurs

- Aptitudes Méditerranée
