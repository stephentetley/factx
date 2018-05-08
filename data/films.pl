% films.pl


director("The Invisible Life", "Vitor Goncalves").
director("Vendredi Soir", "Claire Denis").
director("The Captive", "Chantal Akerman").
director("Les Yeux Claire", "Jerome Bonnell").
star("Les Yeux Claire", "Nathalie Boutefou").
star("Vendredi Soir", "Valarie Lemercier").
star("Vendredi Soir", "Vincent Lindon").
star("The Invisible Life", "Filipe Duarte").
star("The Invisible Life", "Maria Joao Pinho").
star("The Captive", "Silvie Testud").
star("The Captive", "Stanislav Merhar").

% Disjuctive rule
credit(X,Y) :- director(X,Y).
credit(X,Y) :- star(X,Y).