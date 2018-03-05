function selectItem(idVersionSelectionnee) {
    // récupération de la section d'affichage des détails
    var details = document.getElementById('details');

    // récupération de l'id de la sélection
    //var idVersionSelectionnee = idSelection;
    var libelleSujets = '';

    // Gestion des sujets pour la version sélectionnée
    for (var idVersion in contenuVersion) {
        if (contenuVersion.hasOwnProperty(idVersion)) {
            if (idVersion == idVersionSelectionnee) {
                for (var indexSujet = 0; indexSujet < contenuVersion[idVersion].length; indexSujet++) {
                    libelleSujets += '<li>' + contenuVersion[idVersion][indexSujet] + '</li>';
                }
            }
        }
    }
    // Libellé de la sélection (Affiché dans le titre du panel de détails)
    var selection = versionsPrevisionnelles.get(idVersionSelectionnee).content;

    if (selection == null) details.innerHTML = '';
    else {
        var date = versionsPrevisionnelles.get(idVersionSelectionnee).start.split("-");
        // Formattage de la date qui est au format anglais (AAAA-MM-JJ) dans VIS et qu'on affiche en format FR (JJ-MM-AAAA)
        var dateAAfficher = 'J8 : ' + date[2] + '/' + date[1] + '/' + date[0];
        // Affichage centré avec bootstrap
        // Titre du panel (nom version - J8 : jj/mm/aaaa)
        var panelDetail = '<br/><div class="row "><div class="col-md-8 col-lg-offset-2"><div class="panel panel-default"><div class="panel-heading"><h3 class="panel-title">' +
                                 versionsPrevisionnelles.get(idVersionSelectionnee).content + ' - ' + dateAAfficher + '</h3></div>';
        var detail = '';
        // Affichage de la liste des sujets de la version sélectionnée
        // Affichage de la liste des sujets de la version sélectionnée
        if (libelleSujets.length > 0)
            detail = '<ul>' + libelleSujets + '</ul></div>';

        // affichage dans le corps du panel des sujets
        details.innerHTML = panelDetail + '<div class="panel-body">' + detail + '</div></div>';
    }
}


function selectItemCommerciale(idVersionSelectionnee) {
    // récupération de la section d'affichage des détails
    var details = document.getElementById('detailsDernieresSorties');


    var libelleSujets = '';
    // Gestion des sujets pour la version sélectionnée
    for (var idVersion in contenuVersion) {
        if (contenuVersion.hasOwnProperty(idVersion)) {
            if (idVersion == idVersionSelectionnee) {
                for (var indexSujet = 0; indexSujet < contenuVersion[idVersion].length; indexSujet++) {
                    libelleSujets += `<li>${contenuVersion[idVersion][indexSujet]}</li>`;
                }
            }
        }
    }

    // Libellé de la sélection (Affiché dans le titre du panel de détails)
    var selection = versionsCommerciales.get(idVersionSelectionnee).content;

    if (selection == null) details.innerHTML = '';
    else {
        var date = versionsCommerciales.get(idVersionSelectionnee).start.split("-");
        // Formattage de la date qui est au format anglais (AAAA-MM-JJ) dans VIS et qu'on affiche en format FR (JJ-MM-AAAA)
        var dateAAfficher = `J8 : ${date[2]}/${date[1]}/${date[0]}`;
        // Affichage centré avec bootstrap
        // Titre du panel (nom version - J8 : jj/mm/aaaa)
        var panelDetail = `<br/><div class="row "><div class="col-md-8 col-lg-offset-2"><div class="panel panel-default"><div class="panel-heading"><h3 class="panel-title">${
                versionsCommerciales.get(idVersionSelectionnee).content} - ${dateAAfficher}</h3></div>`;
        var detail = '';
        // Affichage de la liste des sujets de la version sélectionnée
        if (libelleSujets.length > 0)
            detail = `<ul>${libelleSujets}</ul></div>`;

        // affichage dans le corps du panel des sujets
        details.innerHTML = panelDetail + '<div class="panel-body">' + detail + '</div></div>';
    }
}