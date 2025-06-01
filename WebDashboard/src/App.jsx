import React, { useState } from 'react';
import { Container, Grid, TextField, Typography } from '@mui/material';
import AuthEndpoints from './components/AuthEndpoints';
import PlayerEndpoints from './components/PlayerEndpoints';
import BetEndpoints from './components/BetEndpoints';

function App() {
  const [jwtToken, setJwtToken] = useState('');

  return (
    <Container style={{ padding: '20px', fontFamily: 'Arial, sans-serif' }}>
      <Typography variant="h4" gutterBottom>
        API Testing Dashboard
      </Typography>
      <Grid container spacing={3}>
        <Grid item xs={12}>
          <TextField
            label="JWT Token"
            fullWidth
            value={jwtToken}
            onChange={(e) => setJwtToken(e.target.value)}
          />
        </Grid>
        <Grid item xs={12} md={4}>
          <AuthEndpoints jwtToken={jwtToken} setJwtToken={setJwtToken} />
        </Grid>
        <Grid item xs={12} md={4}>
          <PlayerEndpoints jwtToken={jwtToken} />
        </Grid>
        <Grid item xs={12} md={4}>
          <BetEndpoints jwtToken={jwtToken} />
        </Grid>
      </Grid>
    </Container>
  );
}

export default App;
