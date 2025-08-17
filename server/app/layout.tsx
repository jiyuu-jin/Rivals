import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import { MantineProvider, ColorSchemeScript } from '@mantine/core';
import '@mantine/core/styles.css';
import "./globals.css";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "Rivals - AR Zombie Game",
  description: "Battle zombies and compete on the global leaderboard",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <head>
        <ColorSchemeScript defaultColorScheme="dark" />
      </head>
      <body className={`${geistSans.variable} ${geistMono.variable}`}>
        <MantineProvider
          defaultColorScheme="dark"
          theme={{
            primaryColor: 'red',
            fontFamily: geistSans.style.fontFamily,
            fontFamilyMonospace: geistMono.style.fontFamily,
            colors: {
              dark: [
                '#C1C2C5',
                '#A6A7AB',
                '#909296',
                '#5C5F66',
                '#373A40',
                '#2C2E33',
                '#25262B',
                '#1A1A1A',
                '#141414',
                '#0F0F0F'
              ],
              red: [
                '#FFE5E5',
                '#FFC1C1',
                '#FF9999',
                '#FF7070',
                '#FF4444',
                '#E53935',
                '#D32F2F',
                '#C62828',
                '#B71C1C',
                '#8B0000'
              ]
            }
          }}
        >
          {children}
        </MantineProvider>
      </body>
    </html>
  );
}
