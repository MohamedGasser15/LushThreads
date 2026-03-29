import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'app_colors.dart';

class AppTextStyles {
  AppTextStyles._();

  static TextStyle get headlineLarge => GoogleFonts.poppins(
        fontSize: 34, fontWeight: FontWeight.w700,
        color: AppColors.textMain, height: 1.1,
      );

  static TextStyle get headlineMedium => GoogleFonts.poppins(
        fontSize: 28, fontWeight: FontWeight.w700,
        color: AppColors.textMain, height: 1.2,
      );

  static TextStyle get titleLarge => GoogleFonts.poppins(
        fontSize: 22, fontWeight: FontWeight.w600,
        color: AppColors.textMain, height: 1.2,
      );

  static TextStyle get titleMedium => GoogleFonts.poppins(
        fontSize: 17, fontWeight: FontWeight.w600,
        color: AppColors.textMain, height: 1.3,
      );

  static TextStyle get bodyLarge => GoogleFonts.urbanist(
        fontSize: 17, fontWeight: FontWeight.w400,
        color: AppColors.textMain, height: 1.5,
      );

  static TextStyle get bodyMedium => GoogleFonts.urbanist(
        fontSize: 15, fontWeight: FontWeight.w400,
        color: AppColors.textMuted, height: 1.4,
      );

  static TextStyle get bodySmall => GoogleFonts.urbanist(
        fontSize: 13, fontWeight: FontWeight.w400,
        color: AppColors.textMuted, height: 1.4,
      );

  static TextStyle get labelLarge => GoogleFonts.urbanist(
        fontSize: 15, fontWeight: FontWeight.w600,
        color: AppColors.textMain, height: 1.3,
      );

  static TextStyle get labelMedium => GoogleFonts.urbanist(
        fontSize: 13, fontWeight: FontWeight.w600,
        color: AppColors.textMain, height: 1.3,
      );

  static TextStyle get labelSmall => GoogleFonts.urbanist(
        fontSize: 11, fontWeight: FontWeight.w700,
        color: AppColors.textMain, height: 1.2,
      );

  static TextStyle get logoStyle => GoogleFonts.poppins(
        fontSize: 22, fontWeight: FontWeight.w900,
        color: AppColors.primary, letterSpacing: -0.5,
      );

  static TextStyle get priceLarge => GoogleFonts.poppins(
        fontSize: 22, fontWeight: FontWeight.w800,
        color: AppColors.accent,
      );

  static TextStyle get priceSmall => GoogleFonts.poppins(
        fontSize: 14, fontWeight: FontWeight.w800,
        color: AppColors.accent,
      );
}
