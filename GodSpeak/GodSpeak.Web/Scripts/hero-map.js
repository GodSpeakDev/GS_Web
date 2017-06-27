var map;
function initMap() {
    // Create the map with no initial style specified.
    // It therefore has default styling.
    map = new google.maps.Map(document.getElementById('map'), {
        center: {lat: 39.9131538, lng: -85.381044},
        zoom: 10,
        mapTypeControl: false,
        scrollwheel:  false,
        disableDefaultUI: true
    });

    // Set the map's style to the initial value of the selector.
    map.setOptions({styles: styles});


    var image = {
        url: '/assets/img/oval.png'
    };

    // Markers
    var p1 = {lat: 39.9131538, lng: -85.381044};

    var p1Marker = new google.maps.Marker({
        position: p1,
        map: map,
        label: {
            text: '1',
            color: 'white',
            fontSize: '20px'
        },
        icon: image
    });

    ///////////////////////

    var p2 = {lat: 40.0057614, lng: -85.5277284};

    var p2Marker = new google.maps.Marker({
        position: p2,
        map: map,
        label: {
            text: '2',
            color: 'white',
            fontSize: '20px'
        },
        icon: image
    });

    ///////////////////////

    var p3 = {lat: 40.001838, lng: -85.331061};

    var p3Marker = new google.maps.Marker({
        position: p3,
        map: map,
        label: {
            text: '3',
            color: 'white',
            fontSize: '20px'
        },
        icon: image
    });

    ///////////////////////

    var p4 = {lat: 39.832935, lng: -85.3532805};

    var p4Marker = new google.maps.Marker({
        position: p4,
        map: map,
        label: {
            text: '4',
            color: 'white',
            fontSize: '20px'
        },
        icon: image
    });

}

var colorGeometry = '#D7D3C4';
var colorTextFill = '#999999';
var colorTextStroke = '#f5f5f5';
var colorRoads = '#B99882';
var colorParks = '#A8AC96';
var colorSmallRoads = '#999999';
var colorPOI = '#D2B48C';
var colorWater = '#B9DBFF';

var styles = [
    {
        elementType: 'geometry',
        stylers: [{color: colorGeometry}]
    },
    {
        elementType: 'labels.icon',
        stylers: [{visibility: 'off'}]
    },
    {
        elementType: 'labels.text.fill',
        stylers: [{color: colorTextFill}]
    },
    {
        elementType: 'labels.text.stroke',
        stylers: [{color: colorTextStroke}]
    },
    {
        featureType: 'administrative.land_parcel',
        elementType: 'labels.text.fill',
        stylers: [{color: '#bdbdbd'}]
    },
    {
        featureType: 'poi',
        elementType: 'geometry',
        stylers: [{color: colorPOI}]
    },
    {
        featureType: 'poi',
        elementType: 'labels.text.fill',
        stylers: [{color: '#757575'}]
    },
    {
        featureType: 'poi.park',
        elementType: 'geometry',
        stylers: [{color: colorParks}]
    },
    {
        featureType: 'poi.park',
        elementType: 'labels.text.fill',
        stylers: [{color: '#9e9e9e'}]
    },
    {
        featureType: 'road',
        elementType: 'geometry',
        stylers: [{color: colorSmallRoads}]
    },
    {
        featureType: 'road.arterial',
        elementType: 'labels.text.fill',
        stylers: [{color: '#757575'}]
    },
    {
        featureType: 'road.highway',
        elementType: 'geometry',
        stylers: [{color: colorRoads}]
    },
    {
        featureType: 'road.highway',
        elementType: 'labels.text.fill',
        stylers: [{color: '#616161'}]
    },
    {
        featureType: 'road.local',
        elementType: 'labels.text.fill',
        stylers: [{color: '#9e9e9e'}]
    },
    {
        featureType: 'transit.line',
        elementType: 'geometry',
        stylers: [{color: '#e5e5e5'}]
    },
    {
        featureType: 'transit.station',
        elementType: 'geometry',
        stylers: [{color: '#eeeeee'}]
    },
    {
        featureType: 'water',
        elementType: 'geometry',
        stylers: [{color: colorWater}]
    },
    {
        featureType: 'water',
        elementType: 'labels.text.fill',
        stylers: [{color: '#9e9e9e'}]
    }
];