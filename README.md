# ConquestTroupFormation
Projet AMJV

---------------------- DEROULE DU JEU----------------------

Le but est d'attaquer et récupérer la couronne adverse.

Phase 1 : Achat
Le joueur peut acheter des unités avec l'argent à sa dispostion et les placer sur sa zone de départ. Il peut également les vendre.

Phase 2 : Combat
Le joueur controle alors les unités individuellement ou en groupe pour aller récuperer le couronne adverse et la ramener devant son trone.
La première unité à récuper la couronne devient le roi et doit revenir dans la zone de départ. Il est plus lent donc une défense est nécessaire.

2 marnière de gagner : Ramener la couronne au trone ou tuer tout les unités adverses.
2 manière de perdre : Ne plus avoir d'unité dans la phase de combat ou la mort du roi.

------------------------ CONTROLES ------------------------

Commandes de la caméra :
- ZQSD : déplacer la caméra;
- Shift : accélérer le déplacement;
- Scroll molette : zoomer / dézoomer.

Commandes de sélection :
- Ctrl + Left Click : sélection d'une unité;
- Ctrl + Left Drag : sélection de zone;
- Ctrl + A : sélection de toutes les unités.

Phase d'achat :
- Left Click : pose une unité (marche aussi en drag);
- Right Click : enlève une unité (marche aussi en drag);
- 1-9 : choix de l'unité;
- Suppr : enlève toutes les unités sélectionnées;
- R : fait tourner la preview de la troupe (ne sert à rien mais c'est joli).

En jeu :
- 1 : indique une position vers laquelle se déplacer;
- 2 : selectionne deux points entre lesquels la troupe va patrouiller;
- 3 : sélectionne une unité à suivre;
- 4 : se met en standby;
- Sift + 1-4 : mettre une action dans la file d'attente;
- F : capacité spéciale.

Catapulte :
- E puis Left Click : sélection de la zone cible.

Bâtisseurs :
- Left Click : sélectionne la position du mur (2 points requis)

Divers : 
- Escape : revenir au menu précédent/mettre en pause


------------------------ BUG CONNUS ------------------------

La catapulte si on lui redonne un ordre garde en mémoire l'ordre précédent et tire deux fois plus
La catapulte continue de tirer même si elle roule

Le batisseur peut se retrouver bloqué dans son pathfinding (on crois)

Le bélier peut avoir du mal à Focus les murs


------------------ PISTES D'AMELIORATION ------------------

Plus d'arène (en cours)
Des ennemis placé aléatoirement tout en suivant une logique dans leur compositon et leur placement du côté adverse.
IA de la catapulte et du bélier adverse à implémenter.