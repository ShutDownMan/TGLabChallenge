import React, { useState } from 'react';
import axios from 'axios';

const API_HOST = 'http://localhost:8080';

function AuthEndpoints({ jwtToken, setJwtToken }) {
  const [response, setResponse] = useState(null);

  const handleRequest = async (endpoint, method, data = null) => {
    try {
      const config = {
        method,
        url: `${API_HOST}${endpoint}`,
        headers: { Authorization: `Bearer ${jwtToken}` },
        data,
      };
      const res = await axios(config);
      if (endpoint === '/api/Auth/login' && method === 'POST') {
        setJwtToken(res.data.token); // Automatically set token on login success
      }
      setResponse(res.data);
    } catch (error) {
      setResponse(error.response?.data || error.message);
    }
  };

  return (
    <div>
      <h2>Auth Endpoints</h2>
      <button onClick={() => handleRequest('/api/Auth/register', 'POST', {
        username: 'jesuisjedi',
        password: '123456Sete',
        email: 'jedson@teste.com',
        currencyId: 1,
        initialBalance: '2345.00',
      })}>
        Register
      </button>
      <button onClick={() => handleRequest('/api/Auth/login', 'POST', {
        identifier: 'jesuisjedi',
        password: '123456Sete',
      })}>
        Login
      </button>
      <button onClick={() => handleRequest('/api/Auth/validate-token', 'POST', { token: jwtToken })}>
        Validate Token
      </button>
      <div>
        <h3>Response:</h3>
        <pre>{JSON.stringify(response, null, 2)}</pre>
      </div>
    </div>
  );
}

export default AuthEndpoints;
