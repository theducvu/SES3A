using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using BackEndConsole3A;

namespace Navigation_App.Assets.BackEndMapboxScripts
{
    public class BackEndMapboxApiLibrary
    {
        public BackEndMapboxApiLibrary() {

        }

        public async Task<string> GetRoute(int profileNumber, double sourceX, double sourceY, double initDestX, double initDestY, params double[] destinationCoordinates) {
            int interval = 0;
            Coordinates sourceCoordinates = new Coordinates() {
                Latitude = sourceX,
                Longitude = sourceY
            };
            List<Coordinates> destinationCoordinateList = new List<Coordinates>();
            destinationCoordinateList.Add(new Coordinates() {
                Latitude = initDestX,
                Longitude = initDestY
            });
            foreach(var destCoord in destinationCoordinates) {
                destinationCoordinateList.Add(new Coordinates() {
                    Latitude = destinationCoordinates[interval],
                    Longitude = destinationCoordinates[interval + 1]
                });
                interval = interval + 2;
            }
            string coordinateString = BuildCoordinatesString(sourceCoordinates, destinationCoordinateList);
            string profile = TranslateProfileNumber(profileNumber);
            string getRouteResponse = await GetRouteTask(profile, coordinateString);
            return getRouteResponse;
        }

        public async Task<string> GetRoute(int profileNumber, string sourceAddress, string initialDestAddress, params string[] destAddresses) {
            List<Coordinates> coordinates = new List<Coordinates>();
            dynamic reverseGeocodingResponseSource = JsonConvert.DeserializeObject(await GetForwardGeocodedCoordinates(sourceAddress));
            Coordinates sourceCoordinates = new Coordinates() {
                Latitude = Convert.ToDouble(reverseGeocodingResponseSource.features[2].center[1]),
                Longitude = Convert.ToDouble(reverseGeocodingResponseSource.features[2].center[0])
            };
            dynamic reverseGeocodingResponseInitDest = JsonConvert.DeserializeObject(await GetForwardGeocodedCoordinates(initialDestAddress));
            Coordinates destCoordinatesInit = new Coordinates() {
                Latitude = Convert.ToDouble(reverseGeocodingResponseInitDest.features[2].center[1]),
                Longitude = Convert.ToDouble(reverseGeocodingResponseInitDest.features[2].center[0])
            };
            foreach(var address in destAddresses) {
                dynamic reverseGeocodingResponse = JsonConvert.DeserializeObject(await GetForwardGeocodedCoordinates(address));
                Coordinates destCoordinates = new Coordinates() {
                Latitude = Convert.ToDouble(reverseGeocodingResponseInitDest.features[2].center[1]),
                Longitude = Convert.ToDouble(reverseGeocodingResponseInitDest.features[2].center[0])
                };
                coordinates.Add(destCoordinates);
            }
            List<double> coordinatePairs = new List<double>();
            for(int i = 0; i < coordinates.Count; i++) {
                if(i > 1) {
                    Coordinates currentCoordinates = coordinates[i];
                    coordinatePairs.Add(currentCoordinates.Latitude);
                    coordinatePairs.Add(currentCoordinates.Longitude);
                }
            }
            double[] varargsArray = coordinatePairs.ToArray();
            string getRouteResponse = await GetRoute(profileNumber, sourceCoordinates.Latitude, sourceCoordinates.Longitude, destCoordinatesInit.Latitude, destCoordinatesInit.Longitude, varargsArray);
            return getRouteResponse;
        }

        public async Task<string> GetForwardGeocodedCoordinates(string placeName, int endpointType = 0) {
            string endpoint = TranslateEndpointNumber(endpointType);
            string getForwardGeocodedResponse = await GetForwardGeocodedCoordinatesTask(placeName, endpoint);
            return getForwardGeocodedResponse;
        }

        public async Task<string> GetReverseGeocodedPlaceName(double xCoord, double yCoord, int endpointType = 0) {
            string endpoint = TranslateEndpointNumber(endpointType);
            Coordinates coordinates = new Coordinates() {
                Latitude = xCoord,
                Longitude = yCoord
            };
            string getReverseGeocodedPlaceName = await GetReverseGeocodedPlaceNameTask(coordinates, endpoint);
            return getReverseGeocodedPlaceName;
        }

        private string TranslateProfileNumber(int profileNumber) {
            switch(profileNumber) {
                case 0: return "driving-traffic";
                case 1: return "driving";
                case 2: return "walking";
                case 3: return "cycling";
                default: throw new InvalidProfileNumberException("The profile number passed in was not between 0 and 3");
            }
        }

        private string TranslateEndpointNumber(int endpointNumber) {
            switch(endpointNumber) {
                case 0: return "mapbox.places";
                case 1: return "mapbox.places-permanent";
                default: return "mapbox.places"; // No need to throw exception, just default to case 0.
            }
        }

        private string BuildCoordinatesString(Coordinates sourceCoords, List<Coordinates> coordinatePairs) {
            string coordinateString = "";
            coordinateString = (coordinatePairs.Count == 0) ? 
            coordinateString + $"{sourceCoords.Longitude},{sourceCoords.Latitude}"
            : coordinateString + $"{sourceCoords.Longitude},{sourceCoords.Latitude};";
            for (int i = 0; i < coordinatePairs.Count; i++) {
                Coordinates coordinates = coordinatePairs[i];
                coordinateString = (i != coordinatePairs.Count - 1) 
                ? coordinateString + $"{coordinates.Longitude},{coordinates.Latitude};" 
                : coordinateString = coordinateString + $"{coordinates.Longitude},{coordinates.Latitude}";
            }
            return coordinateString;
        }

        private async Task<string> GetRouteTask(string profile, string coords) {
            HttpClient routingClient = new HttpClient();
            var routingClientResponse = await routingClient.GetAsync($"{SystemConfiguration.GENERAL_BASE_API_URL}{SystemConfiguration.DIRECTIONS_API_BASE_URL}{profile}/{coords}?{SystemConfiguration.TOKEN_REQUEST_PARAM}{SystemConfiguration.MAIN_TOKEN}");
            if (routingClientResponse.StatusCode == HttpStatusCode.OK) {
                var content = await routingClientResponse.Content.ReadAsStringAsync();
                dynamic routingResponse = JsonConvert.DeserializeObject(content);
                return Convert.ToString(routingResponse);
            }
            return "";
        }

        private async Task<string> GetForwardGeocodedCoordinatesTask(string placeName, string endpoint) {
            HttpClient forwardGeocodingClient = new HttpClient();
            var forwardGeocodingClientResponse = await forwardGeocodingClient.GetAsync($"{SystemConfiguration.GENERAL_BASE_API_URL}{SystemConfiguration.FORWARD_GEOCODING_API_BASE_URL}{endpoint}/{placeName}.json?{SystemConfiguration.TOKEN_REQUEST_PARAM}{SystemConfiguration.MAIN_TOKEN}");
            if (forwardGeocodingClientResponse.StatusCode == HttpStatusCode.OK) {
                var content = await forwardGeocodingClientResponse.Content.ReadAsStringAsync();
                dynamic forwardGeocodingResponse = JsonConvert.DeserializeObject(content);
                return Convert.ToString(forwardGeocodingResponse);
            }
            return "";
        }

        private async Task<string> GetReverseGeocodedPlaceNameTask(Coordinates coordinates, string endpoint) {
            HttpClient reverseGeocodingClient = new HttpClient();
            var reverseGeocodingClientResponse = await reverseGeocodingClient.GetAsync($"{SystemConfiguration.GENERAL_BASE_API_URL}{SystemConfiguration.FORWARD_GEOCODING_API_BASE_URL}{endpoint}/{coordinates.Longitude},{coordinates.Latitude}.json?{SystemConfiguration.TOKEN_REQUEST_PARAM}{SystemConfiguration.MAIN_TOKEN}");
            if (reverseGeocodingClientResponse.StatusCode == HttpStatusCode.OK) {
                var content = await reverseGeocodingClientResponse.Content.ReadAsStringAsync();
                dynamic reverseGeocodingResponse = JsonConvert.DeserializeObject(content);
                return Convert.ToString(reverseGeocodingResponse);
            }
            return "";
        }
    }
}