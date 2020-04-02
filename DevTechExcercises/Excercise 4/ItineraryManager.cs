using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace DevTechExcercises.Excercise_4
{   
    public class ItineraryManager
    {
        private readonly IDataStore _dataStore;
        private readonly IDistanceCalculator _distanceCalculator;

        /*
         * Class can be decoupled by injecting IDataStore and IDistanceCalculator into constructor, instead of creating them in constructor
         * "SqlDbConnection" and "GoogleMapsApiKey" literals can be stored separately in constants if shared
         */
        public ItineraryManager()
        {
            _dataStore = new SqlAgentStore(ConfigurationManager.ConnectionStrings["SqlDbConnection"].ConnectionString); //can be improved
            _distanceCalculator = new GoogleMapsDistanceCalculator(ConfigurationManager.AppSettings["GoogleMapsApiKey"]);//can be improved
        }

        /*
         * Method can be made async, thus allowing to await GetItinaryAsync call
         * List<Quote> results must be replaced with a thread-safe collection such as ConcurrentBag
         */
        public IEnumerable<Quote> CalculateAirlinePrices(int itineraryId, IEnumerable<IAirlinePriceProvider> priceProviders)
        {
            var itinerary = _dataStore.GetItinaryAsync(itineraryId).Result; //can be improved, Result is a blocking call
            if (itinerary == null)
                throw new InvalidOperationException();

            List<Quote> results = new List<Quote>(); //this can cause a race condition, a thread-safe version of List must be used
            Parallel.ForEach(priceProviders, provider =>
            {
                var quotes = provider.GetQuotes(itinerary.TicketClass, itinerary.Waypoints);
                foreach (var quote in quotes)
                    results.Add(quote);
            });
            return results;
        }
                
        public async Task<double> CalculateTotalTravelDistanceAsync(int itineraryId)
        {
            var itinerary = await _dataStore.GetItinaryAsync(itineraryId);
            if (itinerary == null)
                throw new InvalidOperationException();
            double result = 0;
            for (int i = 0; i < itinerary.Waypoints.Count - 1; i++)
            {
                result = result + _distanceCalculator.GetDistanceAsync(itinerary.Waypoints[i],
                     itinerary.Waypoints[i + 1]).Result;
                //.Result is a blocking call in an async method
                //await is missing
                //result is not added up in a thread-safe manner
            }
            return result; 
        }

        //Optimised
        public async Task<double> CalculateTotalTravelDistanceAsync(int itineraryId)
        {
            var itinerary = await _dataStore.GetItinaryAsync(itineraryId);
            if (itinerary == null)
                throw new InvalidOperationException();
            
            var tasks = new List<Task<double>>();
            for (int i = 0; i < itinerary.Waypoints.Count - 1; i++)
            {
                tasks.Add(_distanceCalculator.GetDistanceAsync(itinerary.Waypoints[i], itinerary.Waypoints[i + 1]));         
            }

            var result = await Task.WhenAll(tasks);

            return result.Sum();
        }

        /*
         * Method name contradicts its signature and logic. Method is called FindAgent, which means it's supposed to just find, not update anything.
         * Phone number must be checked and throw ArgumentNullException before retrieving the agent from datastore, this would safe a roundtrip to db if number is missing      
        */        
        public TravelAgent FindAgent(int id, string updatedPhoneNumber)
        {
            var agentDao = _dataStore.GetAgent(id);
            if (agentDao == null)
                return null;
            if (!string.IsNullOrWhiteSpace(updatedPhoneNumber))
            {
                agentDao.PhoneNumber = updatedPhoneNumber;
                _dataStore.UpdateAgent(id, agentDao);
            }
            return Mapper.Map<TravelAgent>(agentDao);
        }
    }
}
