import React, { useState } from 'react';
import AuthEndpoints from './components/AuthEndpoints';
import PlayerEndpoints from './components/PlayerEndpoints';
import BetEndpoints from './components/BetEndpoints';

function App() {
  const [jwtToken, setJwtToken] = useState('');

  return (
    <div style={{ padding: '20px', fontFamily: 'Arial, sans-serif' }}>
      <h1>API Testing Dashboard</h1>
      <div style={{ marginBottom: '20px' }}>
        <label>
          JWT Token:
          <input
            type="text"
            value={jwtToken}
            onChange={(e) => setJwtToken(e.target.value)}
            style={{ marginLeft: '10px', width: '400px' }}
          />
        </label>
      </div>
      <AuthEndpoints jwtToken={jwtToken} setJwtToken={setJwtToken} />
      <PlayerEndpoints jwtToken={jwtToken} />
      <BetEndpoints jwtToken={jwtToken} />
    </div>
  );
}

export default App;
