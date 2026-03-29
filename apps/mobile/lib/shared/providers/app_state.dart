class AppState {
  AppState._();
  static final AppState instance = AppState._();

  bool hasSeenOnboarding = false;
  bool isLoggedIn        = false;
  bool isGuest           = false;

  void completeOnboarding() => hasSeenOnboarding = true;

  void loginAsGuest() {
    isGuest   = true;
    isLoggedIn = false;
  }

  void loginAsUser() {
    isLoggedIn = true;
    isGuest    = false;
  }

  void logout() {
    isLoggedIn = false;
    isGuest    = false;
  }
}
