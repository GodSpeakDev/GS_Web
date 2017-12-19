var map;
function initMap() {
    // Create the map with no initial style specified.
    // It therefore has default styling.
    map = new google.maps.Map(document.getElementById('map'), {
        center: getCenter(),
        zoom: 6,
        mapTypeControl: false,
        scrollwheel:  true,
        disableDefaultUI: true
    });

    // Set the map's style to the initial value of the selector.
    map.setOptions({styles: styles});


    var image = {
        url: '/Content/Images/oval.png',
        

    };

    initMarkers(map, image);

}
//
//var colorGeometry = '#D7D3C4';
//var colorTextFill = '#999999';
//var colorTextStroke = '#f5f5f5';
//var colorRoads = '#B99882';
//var colorParks = '#A8AC96';
//var colorSmallRoads = '#999999';
//var colorPOI = '#D2B48C';
//var colorWater = '#B9DBFF';
//
var styles = [
   
];