﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using MgcPrxyDrftr.models;
using Card = ScryfallApi.Client.Models.Card;
using Set = ScryfallApi.Client.Models.Set;
using Newtonsoft.Json;
using System.Net;
using OpenBoostersAPI;

namespace MgcPrxyDrftr
{
    public class ApiCaller
    {
        private const int Timeout = 30;
        private readonly HttpClient _client;
        private readonly HttpClient _openBoostersClient;

#if DEBUG
        private const string ApiKey = "00000000-0000-0000-0000-000000000000";
        //private const string AdditionalRequestUri = "dev/";
        private const string AdditionalRequestUri = "";
#else
        private const string ApiKey = "418fed9c-1aa0-4628-87d0-45ec41af03f5";
        private const string AdditionalRequestUri = "";
#endif

        // https://scryfall.com/docs/api

        public ApiCaller()
        {
            _client = new HttpClient { BaseAddress = new Uri("https://api.scryfall.com") };
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            _openBoostersClient = new HttpClient(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });
            _openBoostersClient.BaseAddress = new Uri($"https://lotterstedt.de/{AdditionalRequestUri}");
            _openBoostersClient.Timeout = TimeSpan.FromSeconds(Timeout);
            _openBoostersClient.DefaultRequestHeaders.Add("X-API-KEY", ApiKey);
            _openBoostersClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<OpenBoosterBox> GenerateBooster(string setCode, BoosterType boosterType, int amount)
        {
            var response = await _openBoostersClient.GetAsync($"{setCode}/{Enum.GetName(boosterType)!.ToLower()}/{amount}");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<OpenBoosterBox>(json);
        }

        public async Task<Card> GetCardByNameAsync(string cardName, string setCode = "")
        {
            var response = await _client.GetAsync($"cards/named?exact={cardName}&set={setCode}");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Card>(json);
        }

        public async Task<Card> GetCardByNameAndLanguageAsync(string cardName, string language, string setCode = "")
        {
            //https://scryfall.com/search?order=released&q=lang%3Ade&unique=prints
            //https://api.scryfall.com/cards/search?q=name%3DHellkite+set%3DGRN+lang%3Dde
            var response = await _client.GetAsync($"cards/search?q=name%3D{cardName}+set%3D{setCode}+lang%3D{language}");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Card>(json);
        }

        public async Task<Card> GetCardByMultiverseIdAsync(string multiverseid)
        {
            var response = await _client.GetAsync($"cards/multiverse/{multiverseid}");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Card>(json);
        }

        public async Task<Card> GetCardByScryfallIdAsync(Guid scryfallGuid)
        {
            return await GetCardByScryfallIdAsync(scryfallGuid.ToString());
        }
        public async Task<Card> GetCardByScryfallIdAsync(string scryfallGuid)
        {
            var response = await _client.GetAsync($"cards/{scryfallGuid}");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Card>(json);
        }

        public async Task<List<Set>> GetSetsAsync()
        {
            var response = await _client.GetAsync("sets");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Set>>(json);
        }
    }
}
