using Newtonsoft.Json;
using System.Security.Cryptography;

namespace GlobalExpressEntryDotnet
{
  /// <summary>
  /// Global Express Entry looks up address data from partial input, returning
  /// matching address records (address line, city, state, postal code, suite
  /// information, address keys, and so on) for the address fields supplied.
  ///
  /// <para>High-level flow of this sample:</para>
  /// <list type="number">
  ///   <item><description>ARGS    - ParseArguments reads any --flag values off the command line.</description></item>
  ///   <item><description>INPUT   - CallAPI fills in whatever wasn't supplied via interactive prompts.</description></item>
  ///   <item><description>REQUEST - CallAPI builds the REST query string (license + input fields).</description></item>
  ///   <item><description>CALL    - GetContents issues the GET request and pretty-prints the JSON response.</description></item>
  /// </list>
  ///
  /// <para>This sample is a thin HTTP client: it builds a query string, sends a GET
  /// request to the Global Express Entry Cloud API, and prints the JSON response.</para>
  ///
  /// <para>Reference:</para>
  /// <list type="bullet">
  ///   <item><description>Documentation: https://docs.melissa.com/cloud-api/global-express-entry/global-express-entry-index.html</description></item>
  ///   <item><description>Release notes: https://releasenotes.melissa.com/cloud-api/global-express-entry/</description></item>
  ///   <item><description>Result codes: https://docs.melissa.com/melissa/result-codes/result-codes-index.html</description></item>
  /// </list>
  /// </summary>
  static class Program
  {
    /// <summary>
    /// Entry point. Reads the optional command-line arguments, then hands control to
    /// CallAPI, which performs the actual request/response cycle.
    /// </summary>
    /// <param name="args">The raw command-line arguments.</param>
    static void Main(string[] args)
    {
      string baseServiceUrl = @"https://expressentry.melissadata.net/";
      string serviceEndpoint = @"web/ExpressAddress"; 
      string license = "";
      string addressline1 = "";
      string city = "";
      string state = "";
      string postal = "";

      // Populate any values passed on the command line, then run the lookup.
      ParseArguments(ref license, ref addressline1, ref city, ref state, ref postal, args);
      CallAPI(baseServiceUrl, serviceEndpoint, license, addressline1, city, state, postal);
    }

    /// <summary>
    /// Reads the supported command-line options and writes each recognized value into
    /// its matching by-ref parameter. Any parameter left unset here falls back to an
    /// interactive prompt later in <see cref="CallAPI"/>.
    ///
    /// <para>Recognized flags (each followed by its value, e.g. "--city Rancho Santa Margarita"):
    /// --license/-l, --addressline1, --city, --state, --postal.</para>
    /// </summary>
    /// <param name="license">Receives the Melissa license string, if supplied.</param>
    /// <param name="addressline1">Receives the address line 1 to look up, if supplied.</param>
    /// <param name="city">Receives the city to look up, if supplied.</param>
    /// <param name="state">Receives the state to look up, if supplied.</param>
    /// <param name="postal">Receives the postal code to look up, if supplied.</param>
    /// <param name="args">The raw command-line arguments to parse.</param>
    static void ParseArguments(ref string license, ref string addressline1, ref string city, ref string state, ref string postal, string[] args)
    {
      for (int i = 0; i < args.Length; i++)
      {
        if (args[i].Equals("--license") || args[i].Equals("-l"))
        {
          if (args[i + 1] != null)
          {
            license = args[i + 1];
          }
        }
        if (args[i].Equals("--addressline1"))
        {
          if (args[i + 1] != null)
          {
            addressline1 = args[i + 1];
          }
        }
        if (args[i].Equals("--city"))
        {
          if (args[i + 1] != null)
          {
            city = args[i + 1];
          }
        }
        if (args[i].Equals("--state"))
        {
          if (args[i + 1] != null)
          {
            state = args[i + 1];
          }
        }
        if (args[i].Equals("--postal"))
        {
          if (args[i + 1] != null)
          {
            postal = args[i + 1];
          }
        }
      }
    }

    /// <summary>
    /// Issues the GET request against the Global Express Entry endpoint and
    /// pretty-prints the API call and the JSON response to the console.
    /// </summary>
    /// <param name="baseServiceUrl">The Global Express Entry Cloud API base URL.</param>
    /// <param name="requestQuery">The endpoint path plus query string built by <see cref="CallAPI"/>.</param>
    public static async Task GetContents(string baseServiceUrl, string requestQuery)
    {
      HttpClient client = new HttpClient();
      client.BaseAddress = new Uri(baseServiceUrl);
      HttpResponseMessage response = await client.GetAsync(requestQuery);

      string text = await response.Content.ReadAsStringAsync();

      // Re-serialize with indentation so the raw response is easier to read.
      var obj = JsonConvert.DeserializeObject(text);
      var prettyResponse = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);

      // Print output
      Console.WriteLine("\n====================================== OUTPUT =====================================\n");

      Console.WriteLine("API Call: ");
      string APICall = Path.Combine(baseServiceUrl, requestQuery);
      for (int i = 0; i < APICall.Length; i += 70)
      {
        if (i + 70 < APICall.Length)
        {
          Console.WriteLine(APICall.Substring(i, 70));
        }
        else
        {
          Console.WriteLine(APICall.Substring(i, APICall.Length - i));
        }
      }

      Console.WriteLine("\nAPI Response:");
      Console.WriteLine(prettyResponse);
    }
    
    /// <summary>
    /// Drives the interactive/CLI loop: gathers the required address fields, builds and
    /// submits the REST query, prints the result, and optionally repeats for another record.
    ///
    /// <para>In interactive mode (no address args supplied) it loops, asking for a new record each pass
    /// until the user answers "N". In one-shot mode (address args supplied) it runs a single
    /// pass and exits.</para>
    /// </summary>
    /// <param name="baseServiceUrl">The Global Express Entry Cloud API base URL.</param>
    /// <param name="serviceEndPoint">The specific Global Express Entry endpoint path to call.</param>
    /// <param name="license">The Melissa license string sent with every request.</param>
    /// <param name="addressline1">An address line 1 to look up in one-shot mode; if all address fields are empty, the program prompts interactively.</param>
    /// <param name="city">A city to look up in one-shot mode.</param>
    /// <param name="state">A state to look up in one-shot mode.</param>
    /// <param name="postal">A postal code to look up in one-shot mode.</param>
    static void CallAPI(string baseServiceUrl, string serviceEndPoint, string license, string addressline1, string city, string state, string postal)
    {
      Console.WriteLine("\n================ WELCOME TO MELISSA GLOBAL EXPRESS ENTRY CLOUD API ================\n");
      
      bool shouldContinueRunning = true;
      while (shouldContinueRunning)
      {
        string inputAddressLine1 = "";
        string inputCity = "";
        string inputState = "";
        string inputPostal = "";

        // No values were supplied via command line, so prompt for every field.
        if (string.IsNullOrEmpty(addressline1) && string.IsNullOrEmpty(city) && string.IsNullOrEmpty(state) && string.IsNullOrEmpty(postal))
        {
          Console.WriteLine("\nFill in each value to see results");

          Console.Write("AddressLine1: ");
          inputAddressLine1 = Console.ReadLine();

          Console.Write("City: ");
          inputCity = Console.ReadLine();

          Console.Write("State: ");
          inputState = Console.ReadLine();

          Console.Write("PostalCode: ");
          inputPostal = Console.ReadLine();
        }
        else
        {
          // At least one field was supplied via command line; use those values as-is.
          inputAddressLine1 = addressline1;
          inputCity = city;
          inputState = state;
          inputPostal = postal;
        }

        // Prompt individually for any still-missing required field.
        while (string.IsNullOrEmpty(inputAddressLine1) || string.IsNullOrEmpty(inputCity) || string.IsNullOrEmpty(inputState) || string.IsNullOrEmpty(inputPostal))
        {
          Console.WriteLine("\nFill in missing required parameter");

          if (string.IsNullOrEmpty(inputAddressLine1))
          {
            Console.Write("AddressLine1: ");
            inputAddressLine1 = Console.ReadLine();
          }

          if (string.IsNullOrEmpty(inputCity))
          {
            Console.Write("City: ");
            inputCity = Console.ReadLine();
          }

          if (string.IsNullOrEmpty(inputState))
          {
            Console.Write("State: ");
            inputState = Console.ReadLine();
          }

          if (string.IsNullOrEmpty(inputPostal))
          {
            Console.Write("PostalCode: ");
            inputPostal = Console.ReadLine();
          }
        }

        // Map input fields to the API's expected query parameter names and
        // request a JSON response.
        Dictionary<string, string> inputs = new Dictionary<string, string>()
        {
            { "format", "json"},
            { "line1", inputAddressLine1},
            { "city", inputCity},
            { "state", inputState},
            { "postalcode", inputPostal}   
        };

        Console.WriteLine("\n======================================= INPUTS ====================================\n");
        Console.WriteLine($"\t   Base Service Url: {baseServiceUrl}");
        Console.WriteLine($"\t  Service End Point: {serviceEndPoint}");
        Console.WriteLine($"\t     Address Line 1: {inputAddressLine1}");
        Console.WriteLine($"\t               City: {inputCity}");
        Console.WriteLine($"\t              State: {inputState}");
        Console.WriteLine($"\t         PostalCode: {inputPostal}");

        // Create Service Call
        // Set the License String in the Request
        string RESTRequest = "";

        RESTRequest += @"&id=" + Uri.EscapeDataString(license);

        // Set the Input Parameters
        foreach (KeyValuePair<string, string> kvp in inputs)
          RESTRequest += @"&" + kvp.Key + "=" + Uri.EscapeDataString(kvp.Value);

        // Build the final REST String Query
        RESTRequest = serviceEndPoint + @"?" + RESTRequest;

        // Submit to the Web Service. 
        bool success = false;
        int retryCounter = 0;

        do
        {
          try //retry just in case of network failure
          {
            GetContents(baseServiceUrl, $"{RESTRequest}").Wait();
            Console.WriteLine();
            success = true;
          }
          catch (Exception ex)
          {
            retryCounter++;
            Console.WriteLine(ex.ToString());
            return;
          }
        } while ((success != true) && (retryCounter < 5));

        // If any address field came from the command line, treat this as a one-shot
        // run rather than looping for additional records.
        bool isValid = false;
        if (!string.IsNullOrEmpty(addressline1 + city + state + postal))
        {
          isValid = true;
          shouldContinueRunning = false;
        }

        // Otherwise ask whether to test another record. Keep prompting until we get a
        // valid Y/N. "N" ends the program; "Y" falls through to another pass.
        while (!isValid)
        {
          Console.WriteLine("\nTest another record? (Y/N)");
          string testAnotherResponse = Console.ReadLine();

          if (!string.IsNullOrEmpty(testAnotherResponse))
          {
            testAnotherResponse = testAnotherResponse.ToLower();
            if (testAnotherResponse == "y")
            {
              isValid = true;
            }
            else if (testAnotherResponse == "n")
            {
              isValid = true;
              shouldContinueRunning = false;
            }
            else
            {
              Console.Write("Invalid Response, please respond 'Y' or 'N'");
            }
          }
        }
      }
      
      Console.WriteLine("\n======================= THANK YOU FOR USING MELISSA CLOUD API =====================\n");
    }
  }
}
