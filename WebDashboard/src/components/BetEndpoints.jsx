import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { API_HOST } from '../config/api';

function BetEndpoints({ jwtToken, selectedBetId }) {
  const [response, setResponse] = useState(null);
  const [selectedWallet, setSelectedWallet] = useState('');
  const [playerProfile, setPlayerProfile] = useState(null);
  const [betId, setBetId] = useState(selectedBetId);
  const [availableBets, setAvailableBets] = useState([]);

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const res = await axios.get(`${API_HOST}/api/Player/profile`, {
          headers: { Authorization: `Bearer ${jwtToken}` }
        });
        setPlayerProfile(res.data);
      } catch (error) {
        console.error('Failed to fetch profile:', error);
      }
    };
    fetchProfile();
  }, [jwtToken]);

  useEffect(() => {
    const fetchBets = async () => {
      try {
        const res = await axios.get(`${API_HOST}/api/Player/bets`, {
          headers: { Authorization: `Bearer ${jwtToken}` }
        });
        console.log('Fetched player bets:', res.data); // Debugging log
        setAvailableBets(res.data);
      } catch (error) {
        console.error('Failed to fetch player bets:', error);
      }
    };
    fetchBets();
  }, [jwtToken]);

  useEffect(() => {
    setBetId(selectedBetId);
  }, [selectedBetId]);

  const handleRequest = async (endpoint, method, data = null) => {
    try {
      const config = {
        method,
        url: `${API_HOST}${endpoint}`,
        headers: { Authorization: `Bearer ${jwtToken}` },
        data,
      };
      const res = await axios(config);
      if (method === 'POST' && endpoint === '/api/Bet') {
        setBetId(res.data.id);
      }
      setResponse(res.data);
    } catch (error) {
      setResponse(error.response?.data || error.message);
    }
  };

  const refreshWallets = async () => {
    try {
      const res = await axios.get(`${API_HOST}/api/Player/profile`, {
        headers: { Authorization: `Bearer ${jwtToken}` }
      });
      setPlayerProfile(res.data);
    } catch (error) {
      console.error('Failed to refresh wallets:', error);
    }
  };

  const refreshBets = async () => {
    try {
      const res = await axios.get(`${API_HOST}/api/Player/bets`, {
        headers: { Authorization: `Bearer ${jwtToken}` }
      });
      setAvailableBets(res.data);
    } catch (error) {
      console.error('Failed to refresh bets:', error);
    }
  };

  return (
    <div>
      <h2>Bet Endpoints</h2>
      <div style={{ marginBottom: '20px' }}>
        <label htmlFor="wallet-select">Select Wallet: </label>
        <select
          id="wallet-select"
          value={selectedWallet}
          onChange={(e) => setSelectedWallet(e.target.value)}
        >
          <option value="">Select a wallet</option>
          {playerProfile?.wallets?.map(wallet => (
            <option key={wallet.id} value={wallet.id}>
              {wallet.name || `Wallet ${wallet.id}`}
            </option>
          ))}
        </select>
      </div>
      <div style={{ marginBottom: '20px' }}>
        <label htmlFor="bet-select">Select Bet: </label>
        <select
          id="bet-select"
          value={betId}
          onChange={(e) => setBetId(e.target.value)}
        >
          <option value="">Select a bet</option>
          {availableBets?.map(bet => (
            <option key={bet.id} value={bet.id}>
              {`Bet ${bet.id} - ${bet.status}`}
            </option>
          ))}
        </select>
      </div>

      <button
        onClick={() => handleRequest('/api/Bet', 'POST', {
          walletId: selectedWallet,
          gameId: '7558398b-a987-4b88-9010-c026306d3535',
          amount: 100,
          currencyId: 1,
          createdAt: '2024-06-01T00:00:00Z',
        })}
        disabled={!selectedWallet}
      >
        Place Bet
      </button>
      <button
        onClick={() => handleRequest(`/api/Bet/${betId}/cancel`, 'POST', {
          cancelReason: 'User requested cancellation',
        })}
        disabled={!betId}
      >
        Cancel Bet
      </button>
      <button
        onClick={() => handleRequest(`/api/Bet/${betId}/settle`, 'POST')}
        disabled={!betId}
      >
        Settle Bet
      </button>
      <button
        onClick={() => handleRequest(`/api/Bet/${betId}`, 'GET')}
        disabled={!betId}
      >
        Get Bet by ID
      </button>
      <button onClick={refreshWallets} disabled={!jwtToken}>
        Refresh Wallets
      </button>
      <button onClick={refreshBets} disabled={!jwtToken}>
        Refresh Bets
      </button>
      <div>
        <h3>Response:</h3>
        <pre>{JSON.stringify(response, null, 2)}</pre>
      </div>
    </div>
  );
}

export default BetEndpoints;
