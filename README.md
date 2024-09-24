```
/* This implements a Tausworthe PRNG with period 2^223. Based on:
**   Tables of maximally-equidistributed combined LFSR generators,
**   Pierre L'Ecuyer, 1991, table 3, 1st entry.
** Full-period ME-CF generator with L=64, J=4, k=223, N1=49.
**
** Important note: This PRNG is NOT suitable for cryptographic use!
**
** But it works fine for math.random(), which has an API that's not
** suitable for cryptography, anyway.
**
** When used as a securely seeded global PRNG, it substantially raises
** the difficulty for various attacks on the VM.
*/
```
https://www.ams.org/journals/mcom/1999-68-225/S0025-5718-99-01039-X/S0025-5718-99-01039-X.pdf  
https://www.ams.org/journals/mcom/1996-65-213/S0025-5718-96-00696-5/S0025-5718-96-00696-5.pdf

### How to use
```cs
// Instantiate random number generator
GmodRandom gmodRnd = new GmodRandom();

// Seed the rng
gmodRnd.mathrandomseed(1722039290);

// Print the state
gmodRnd.PrintStates();

// No arguments: returns a uniform pseudo-random real number in the range 0 to 1 which includes 0 but excludes 1
Log.Info(gmodRnd.mathrandom());

// 1 argument: returns a uniform pseudo-random integer in the range 1 to m inclusive
Log.Info(gmodRnd.mathrandom(10));

// 2 arguments: returns a uniform pseudo-random integer in the range m to n inclusive
Log.Info(gmodRnd.mathrandom(5, 10));
```

If you don't seed the PRNG, it will be seeded with the Unix epoch time (seconds since January 1, 1970) just like gmod.