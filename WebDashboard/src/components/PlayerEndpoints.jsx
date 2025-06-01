import React, { useState } from 'react';
import axios from 'axios';
import { API_HOST } from '../config/api';

function PlayerEndpoints({ jwtToken }) {
  const [response, setResponse] = useState(null);

  const handleRequest = async (endpoint, method) => {
    try {
      const config = {
        method,
        url: `${API_HOST}${endpoint}`,
        headers: { Authorization: `Bearer ${jwtToken}` },
      };
      const res = await axios(config);
      setResponse(res.data);
    } catch (error) {
      setResponse(error.response?.data || error.message);
    }
  };

  return (
    <div>
      <h2>Player Endpoints</h2>
      <button onClick={() => handleRequest('/api/Player/profile', 'GET')}>
        Get Player Profile
      </button>
      <button onClick={() => handleRequest('/api/Player/bets', 'GET')}>
        Get Player Bets
      </button>
      <button onClick={() => handleRequest('/api/Player/wallet-transactions', 'GET')}>
        Get Wallet Transactions
      </button>
      <div>
        <h3>Response:</h3>
        <pre>{JSON.stringify(response, null, 2)}</pre>
      </div>
    </div>
  );
}

export default PlayerEndpoints;
